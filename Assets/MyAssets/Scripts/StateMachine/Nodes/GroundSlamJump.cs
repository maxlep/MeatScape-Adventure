using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class GroundSlamJump : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference JumpImpulse;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference jumpGroundDelay;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private BoolReference hasWaitedJumpDelay;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TriggerVariable triggerJumpAnim;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatValueReference jumpStatMultiplier;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatValueReference jumpHungerMultiplier;


    
    
    public override void Enter()
    {
        base.Enter();
        playerController.UngroundMotor();
        playerController.AddImpulseOverlayed(Vector3.up * JumpImpulse.Value * jumpStatMultiplier.Value * jumpHungerMultiplier.Value);
        triggerJumpAnim.Activate();
    }

    public override void Exit()
    {
        base.Exit();
        hasWaitedJumpDelay.Value = false;
        TimeUtils.SetTimeout(jumpGroundDelay.Value, () => hasWaitedJumpDelay.Value = true);
    }
}
