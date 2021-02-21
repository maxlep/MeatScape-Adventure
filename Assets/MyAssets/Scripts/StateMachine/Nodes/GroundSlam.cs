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

    
    public override void Enter()
    {
        base.Enter();
        
        
        //Set Velocity at least enough to bounce
        StoredJumpVelocity.Value = Mathf.Min(-Mathf.Abs(SlamVelocity.Value), -BounceThresholdVelocity.Value);
        //playerController.AddImpulse(Vector3.down * SlamForce.Value);
    }
}
