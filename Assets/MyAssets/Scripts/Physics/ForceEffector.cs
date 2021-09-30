using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using EnhancedHierarchy.Icons;
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
    [SerializeField] private TransformSceneReference PlayerTransformReference;
    [SerializeField] private Vector3Reference PreviousVelocity;
    [SerializeField] private float ActivateCooldown = .2f;
    [SerializeField] private bool CancelGlide = true;
    [SerializeField] private bool SetVelocity = false;
    [SerializeField] private bool AffectRigidbodies = true;
    [SerializeField] private float RigidodyForce = 1000f;
    [SerializeField] private GameEvent OnForceEffectorActivated;
    [SerializeField] private GameEvent onEffectorCancelGlide;
    
    [SerializeField] [ShowIf("forceType", ForceType.Constant)]
    private float apposingVelocityMultiplier = 1f;

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

    private float lastActivateTime;
    private PlayerController playerController;
    
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

    private void Awake()
    {
        playerController = PlayerTransformReference.Value.GetComponent<PlayerController>();
    }
    

    //To be called by player controller on rigibody sweep
    public void Activate()
    {
        //Prevent double activate from onTriggerEnter and SweepTest
        if (lastActivateTime + ActivateCooldown > Time.time) return;

        lastActivateTime = Time.time;
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
        if (lastActivateTime + ActivateCooldown > Time.time) return;
        
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            Activate();
            return;
        }
        
        //Handle force on rbs other than player
        if (!AffectRigidbodies) return;
        
        if (other.gameObject.layer != LayerMapper.GetLayer(LayerEnum.Player))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                onActivateEffector.Invoke();
                ApplyForceToRigidbody(rb, false);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //Apply constant force to non-player Rbs
        if (other.gameObject.layer != LayerMapper.GetLayer(LayerEnum.Player) && forceType == ForceType.Constant)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                ApplyForceToRigidbody(rb, false);
            }
            return;
        }
            
        //Apply force to player
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
        
        playerController.UngroundMotor();
        Vector3 force = Vector3.zero;

        switch (forceDirectionType)
        {
            case(ForceDirectionType.Directional):
                Vector3 dir = (isDirectionLocalSpace) ? 
                    transform.TransformDirection(direction.normalized) :
                    direction.normalized;
                
                if (useDirectionOfVelocity)
                    dir = PreviousVelocity.Value.normalized;

                float forceMag = forceMagnitude;
            
                //No mult by time.deltaTime because impulse
                if (isConstant)
                {
                    bool isApposingForce = (Vector3.Dot(PreviousVelocity.Value.normalized, dir) < 0f);
                    if (isApposingForce) forceMag *= apposingVelocityMultiplier;
                }
                    
                
                bool isAdditive = forceType == ForceType.Constant;
                playerController.SetImpulseDistance(dir, forceMag, isAdditive, SetVelocity);
                break;
            
            case(ForceDirectionType.Radial):
                Vector3 target = (isTargetLocalSpace) ?
                    transform.TransformPoint(targetPoint)
                    : targetPoint;
            
                Vector3 radialForceDir = (target - playerController.transform.position).normalized;
                force = radialForceDir * radialAcceleration * Time.deltaTime;
                if (isConstant)
                    force *= Time.deltaTime;

                if (SetVelocity) playerController.SetVelocity(force);
                else playerController.AddImpulse(force);
                break;
        }
    }
    
    private void ApplyForceToRigidbody(Rigidbody rb, bool isConstant)
    {
        Vector3 force = Vector3.zero;

        switch (forceDirectionType)
        {
            case(ForceDirectionType.Directional):
                Vector3 dir = (isDirectionLocalSpace) ? 
                    transform.TransformDirection(direction.normalized) :
                    direction.normalized;
                
                if (useDirectionOfVelocity)
                    dir = PreviousVelocity.Value.normalized;

                float forceMag = RigidodyForce;
            
                //No mult by time.deltaTime because impulse
                if (isConstant)
                {
                    bool isApposingForce = (Vector3.Dot(PreviousVelocity.Value.normalized, dir) < 0f);
                    if (isApposingForce) forceMag *= apposingVelocityMultiplier;
                }
                    
                
                bool isAdditive = forceType == ForceType.Constant;
                rb.AddForce(dir * forceMag);
                break;
            
            case(ForceDirectionType.Radial):
                Vector3 target = (isTargetLocalSpace) ?
                    transform.TransformPoint(targetPoint)
                    : targetPoint;
            
                Vector3 radialForceDir = (target - rb.position).normalized;
                force = radialForceDir * RigidodyForce * Time.deltaTime;
                if (isConstant)
                    force *= Time.deltaTime;
                
                rb.AddForce(force);
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
