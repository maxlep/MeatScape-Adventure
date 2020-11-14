using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.ScriptableObjects.Variables;
using Shapes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEngine;

public class ForceEffector : MonoBehaviour
{
    [UnityEngine.Tooltip("Apply force after all player-state logic, overlaid additively. Otherwise send to" +
                         " player states for processing")]
    [SerializeField] private bool isOverlayForce;
    [SerializeField] private ForceType forceType;
    [SerializeField] private ForceDirectionType forceDirectionType;
    [SerializeField] private LayerMapper LayerMapper;

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
    
    [Serializable]
    private enum ForceType
    {
        Constant,
        Impulse
    }

    [Serializable]
    private enum ForceDirectionType
    {
        Directional,
        Radial
    }
    
    
    //TODO: Horiz. force has to be enough to get player out of trigger since additive?
    
    private void OnTriggerEnter(Collider other)
    {
        if (forceType != ForceType.Impulse) return;
        
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            ApplyForceToPlayer(playerController);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (forceType != ForceType.Constant) return;
        
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            ApplyForceToPlayer(playerController);
        }
    }

    private void ApplyForceToPlayer(PlayerController playerController)
    {
        Vector3 force = Vector3.zero;
        playerController.UngroundMotor();

        if (forceDirectionType == ForceDirectionType.Radial)
        {
            Vector3 target = (isTargetLocalSpace) ?
                transform.TransformPoint(targetPoint)
                : targetPoint;
            
            Vector3 radialForceDir = (target - playerController.transform.position).normalized;
            force = radialForceDir * radialAcceleration * Time.deltaTime;
        }
        else if (forceDirectionType == ForceDirectionType.Directional)
        {
            Vector3 dir = (isDirectionLocalSpace) ? 
                transform.TransformDirection(direction.normalized) :
                direction.normalized;
            
            //No mult by time.deltaTime because impulse
            force = forceMagnitude * dir;    
        }
        
        if (isOverlayForce)
            playerController.AddImpulseOverlayed(force);
        else
            playerController.AddImpulse(force);
    }

    private void OnDrawGizmosSelected()
    {
        //TODO: Gizmos to show the forces and directions when selected
        //TODO: Utilize handles to modify properties (Force dir, forceTargetPoint, etc)
        
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
