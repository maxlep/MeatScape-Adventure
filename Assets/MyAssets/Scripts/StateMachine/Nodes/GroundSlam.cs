using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

public class GroundSlam : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatValueReference SlamVelocity;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatValueReference SlamVelocityFast;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference StoredJumpVelocity;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference BounceThresholdVelocity;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private Vector3Reference PreviousVelocity;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TimerVariable GroundSlamCooldownTimer;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference DistanceToGround;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference FastSlamGroundDistanceThreshold;
    
    
    public override void Enter()
    {
        base.Enter();
        GroundSlamCooldownTimer.StartTimer();
        
        //Set Velocity at least enough to bounce, and dont decrease if already falling fast
        
        
        float newJumpVelocity;
        
        //Slam slower if lower above the ground (more time to let go of jump for slam bounce when close to ground)
        if (DistanceToGround.Value >= FastSlamGroundDistanceThreshold.Value)
            newJumpVelocity = Mathf.Min(-Mathf.Abs(SlamVelocityFast.Value), -BounceThresholdVelocity.Value);
        else
            newJumpVelocity = Mathf.Min(-Mathf.Abs(SlamVelocity.Value), -BounceThresholdVelocity.Value);
        
        newJumpVelocity = Mathf.Min(PreviousVelocity.Value.y, newJumpVelocity);
        StoredJumpVelocity.Value = newJumpVelocity;
        //playerController.AddImpulse(Vector3.down * SlamForce.Value);
    }
}
