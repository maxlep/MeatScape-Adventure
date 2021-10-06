using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class RollStick : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected TransformSceneReference TargetTransform;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected Vector3Reference NewVelocity;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected QuaternionReference NewRotation;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected Vector3Reference PreviousVelocityDuringUpdate;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected Vector3Reference PreviousVelocityAfterUpdate;

    public override void Enter()
    {
        base.Enter();
        
        //Set to 0 so velocity isnt carried after exit this state
        NewVelocity.Value = Vector3.zero;
        PreviousVelocityDuringUpdate.Value = Vector3.zero;
        PreviousVelocityAfterUpdate.Value = Vector3.zero;
        
        playerController.onStartUpdateVelocity += UpdateVelocity;
        playerController.onStartUpdateRotation += UpdateRotation;
    }
    
    public override void Exit()
    {
        base.Exit();
        playerController.onStartUpdateVelocity -= UpdateVelocity;
        playerController.onStartUpdateRotation -= UpdateRotation;
    }

    public override void Execute()
    {
        base.Execute();
        playerController.CharacterMotor.SetPositionAndRotation(TargetTransform.Value.position, TargetTransform.Value.rotation);
    }
    
    private void UpdateVelocity(VelocityInfo velocityInfo)
    {
        NewVelocity.Value = Vector3.zero;
    }
	
    private void UpdateRotation(Quaternion currentRotation)
    {
        NewRotation.Value = currentRotation;
    }
}
