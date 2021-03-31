using System.Collections;
using System.Collections.Generic;
using Den.Tools;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class ClumpPitch : PlayerStateNode
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
    protected TransformSceneReference CurrentTargetReference;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
    protected FloatReference DeltaRotationMin;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
    protected TimerVariable ClumpPitchDurationTimer;
    
    #endregion
    
    #region Outputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected Vector3Reference NewVelocityOut;
	
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected QuaternionReference NewRotationOut;

    #endregion

    private float angleToTarget;

    public override void Enter()
    {
        base.Enter();
        playerController.onStartUpdateVelocity += UpdateVelocity;
        playerController.onStartUpdateRotation += UpdateRotation;
        
        //Get angle from current forward to direction of target
        if (CurrentTargetReference.Value != null)
        {
            Vector3 dirToTarget = (CurrentTargetReference.Value.position - playerController.transform.position).xoz().normalized;
            angleToTarget = Vector2.SignedAngle(playerController.transform.forward.xz(), dirToTarget.xz());
            angleToTarget = Mathf.Abs(angleToTarget);
        }
        
    }

    private void UpdateVelocity(VelocityInfo velocityInfo)
    {
        NewVelocityOut.Value = Vector3.zero;
    }
	
    private void UpdateRotation(Quaternion currentRotation)
    {
        //If no target, just keep facing same direction
        if (CurrentTargetReference.Value == null)
        {
            NewRotationOut.Value = currentRotation;
            return;
        }


        Vector3 dirToTarget = (CurrentTargetReference.Value.position - playerController.transform.position).xoz().normalized;
        Quaternion rotToTarget = Quaternion.LookRotation(dirToTarget, Vector3.up);

        //Add enough delta rotation to face target when animation is over.
        float deltaRotation = (angleToTarget / ClumpPitchDurationTimer.Duration) * Time.deltaTime;
        deltaRotation = Mathf.Max(deltaRotation, DeltaRotationMin.Value);
        NewRotationOut.Value = Quaternion.RotateTowards(currentRotation, rotToTarget, deltaRotation);
    }

    public override void Exit()
    {
        base.Exit();
        playerController.onStartUpdateVelocity -= UpdateVelocity;
        playerController.onStartUpdateRotation -= UpdateRotation;
    }
    
}
