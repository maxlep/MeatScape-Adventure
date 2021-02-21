using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class GroundSlam : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference SlamVelocity;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference StoredJumpVelocity;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference BounceThresholdVelocity;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private Vector3Reference PreviousVelocity;
    
    
    public override void Enter()
    {
        base.Enter();
        
        
        //Set Velocity at least enough to bounce, and dont decrease if already falling fast
        float newJumpVelocity = Mathf.Min(-Mathf.Abs(SlamVelocity.Value), -BounceThresholdVelocity.Value);
        newJumpVelocity = Mathf.Min(PreviousVelocity.Value.y, newJumpVelocity);
        StoredJumpVelocity.Value = newJumpVelocity;
        //playerController.AddImpulse(Vector3.down * SlamForce.Value);
    }
}
