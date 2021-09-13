using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Shapes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class ForceEffector : MonoBehaviour
{
    [UnityEngine.Tooltip("Apply force after all player-state logic, overlaid additively. Otherwise send to" +
                         " player states for processing")]
    [SerializeField] private ForceType forceType;
    [SerializeField] private ForceDirectionType forceDirectionType;
    [SerializeField] private LayerMapper LayerMapper;
    [SerializeField] private Vector3Reference PreviousVelocity;
    [SerializeField] private GameEvent OnForceEffectorActivated;
    [SerializeField] private bool CancelGlide = true;
    [SerializeField] private GameEvent onEffectorCancelGlide;
    
    [SerializeField] [ShowIf("forceDirectionType", ForceDirectionType.Directional)]
    private bool useDirectionOfVelocity;

    [SerializeField] [ShowIf("forceDirectionType", ForceDirectionType.Directional)] 
    [LabelText("IsLocalSpace")]
    private bool isDirectionLocalSpace = true;
    
    [SerializeField] [ShowIf("forceDirectionType", ForceDirectionType.Directional)]
    private Vector3 direction;

    [SerializeField] [ShowIf("forceDirectionType", ForceDirectionType.Directional)] 
    [SuffixLabel("m/s")]
    private float forceMagnitude;

    [SerializeField] [HideIf("forceDirectionType", ForceDirectionType.Directional)]
    [LabelText("IsLocalSpace")]
    private bool isTargetLocalSpace = true;

    [SerializeField] [HideIf("forceDirectionType", ForceDirectionType.Directional)]
    private Vector3 targetPoint;

    [SerializeField] [ShowIf("forceDirectionType", ForceDirectionType.Radial)]
    [SuffixLabel("m/s^2")]
    private float radialAcceleration;
    
    [SerializeField]
    private UnityEvent onActivateEffector;

    private bool impulseActived;
    
    [Serializable]
    private enum ForceType
    {
        Constant,
        Impulse,
        Reflect
    }

    [Serializable]
    private enum ForceDirectionType
    {
        Directional,
        Radial
    }

    //To be called by player controller on rigibody sweep
    public void Activate(PlayerController playerController)
    {
        //Prevent double activate from onTriggerEnter and SweepTest
        if (impulseActived) return;

        impulseActived = true;
        onActivateEffector.Invoke();

        switch (forceType)
        {
            case (ForceType.Constant):
                break;
            
            case (ForceType.Impulse):
                OnForceEffectorActivated.Raise();
                if (CancelGlide) onEffectorCancelGlide.Raise();
                ApplyForceToPlayer(playerController, false);
                break;
            
            case (ForceType.Reflect):
                OnForceEffectorActivated.Raise();
                if (CancelGlide) onEffectorCancelGlide.Raise();
                ReflectVelocity(playerController);
                break;
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Prevent double activate from onTriggerEnter and SweepTest
        if (impulseActived) return;
        
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            Activate(playerController);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            //Reset impulse activated flag
            impulseActived = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMapper.GetLayer(LayerEnum.Player))
            return;

        switch (forceType)
        {
            case (ForceType.Constant):
                OnForceEffectorActivated.Raise();
                if (CancelGlide) onEffectorCancelGlide.Raise();
                PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
                ApplyForceToPlayer(playerController, true);
                break;
            
            case (ForceType.Impulse):
                break;
            
            case (ForceType.Reflect):
                break;
            
        }
    }

    private void ApplyForceToPlayer(PlayerController playerController, bool isConstant)
    {
        Vector3 force = Vector3.zero;
        playerController.UngroundMotor();

        switch (forceDirectionType)
        {
            case(ForceDirectionType.Directional):
                Vector3 dir = (isDirectionLocalSpace) ? 
                    transform.TransformDirection(direction.normalized) :
                    direction.normalized;
                
                if (useDirectionOfVelocity)
                    dir = PreviousVelocity.Value.normalized;
            
                //No mult by time.deltaTime because impulse
                force = forceMagnitude * dir;
                if (isConstant)
                    force *= Time.deltaTime;
                
                bool isAdditive = forceType == ForceType.Constant;
                playerController.SetImpulseDistance(dir, forceMagnitude, isAdditive);
                break;
            
            case(ForceDirectionType.Radial):
                Vector3 target = (isTargetLocalSpace) ?
                    transform.TransformPoint(targetPoint)
                    : targetPoint;
            
                Vector3 radialForceDir = (target - playerController.transform.position).normalized;
                force = radialForceDir * radialAcceleration * Time.deltaTime;
                if (isConstant)
                    force *= Time.deltaTime;
                
                playerController.AddImpulse(force);
                break;
        }
    }

    private void ReflectVelocity(PlayerController playerController)
    {
        Vector3 force = Vector3.zero;

        switch (forceDirectionType)
        {
            case(ForceDirectionType.Directional):
                force = Vector3.Reflect(PreviousVelocity.Value, direction.normalized);
                break;
            
            case(ForceDirectionType.Radial):
                Vector3 target = (isTargetLocalSpace) ?
                    transform.TransformPoint(targetPoint)
                    : targetPoint;
            
                Vector3 radialForceDir = (target - playerController.transform.position).normalized;
                force = Vector3.Reflect(PreviousVelocity.Value, -radialForceDir);
                break;
        }
        
        if (force.y > 0f)
            playerController.UngroundMotor();
        
        // if (isOverlayForce)
        //     playerController.AddImpulseOverlayed(force);
        // else
        //     playerController.AddImpulse(force);
        
        playerController.SetImpulseDistance(force.normalized, forceMagnitude);
    }

    private void OnDrawGizmosSelected()
    {
        //TODO: Gizmos to show the forces and directions when selected
        //TODO: Utilize handles to modify properties (Force hitDir, forceTargetPoint, etc)
        
        // set up all static parameters. these are used for all following Draw.Line calls
        Draw.LineGeometry = LineGeometry.Volumetric3D;
        Draw.LineThicknessSpace = ThicknessSpace.Meters;
        Draw.LineThickness = .2f;
        
        //Direction Arrow
        
        //Radial Target Point
        if (forceDirectionType == ForceDirectionType.Radial)
        {
            Color radialTargetColor = new Color(150f/255f, 50f/255f, 220f/255f, 1f);
            Color radialTargetColor2 = new Color(200f/255f, 200f/255f, 50f/255f, .25f);
            Vector3 target = (isTargetLocalSpace) ?
                transform.TransformPoint(targetPoint)
                : targetPoint;
            
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, target, .35f, radialTargetColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, target, .65f, radialTargetColor2);
        }
        else if (forceDirectionType == ForceDirectionType.Directional)
        {
            Color directionColor = new Color(150f/255f, 50f/255f, 220f/255f, 1f);
            float lineLength = 10f;
            Vector3 dir = (isDirectionLocalSpace) ? 
                transform.TransformDirection(direction.normalized) :
                direction.normalized;
            
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + (lineLength * dir);
            float coneRadius = .5f;
            float coneHeight = 1.35f;
            Vector3 coneOffset = -dir * (coneHeight * .5f);
            Quaternion coneRot = Quaternion.LookRotation(dir);

            Draw.Line(startPos, endPos, directionColor);
            if (!Mathf.Approximately(dir.magnitude, 0f)) 
                Draw.Cone(endPos + coneOffset,
                    coneRot, coneRadius, coneHeight, true, directionColor);
        }
        
        
    }
}
