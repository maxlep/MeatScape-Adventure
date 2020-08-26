﻿using Sirenix.OdinInspector;
using UnityEngine;


public class Jump : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] public float timeToJumpApex = .4f;
    [FoldoutGroup("")] [LabelWidth(120)] public float maxJumpHeight = 4f;
    [FoldoutGroup("")] [LabelWidth(120)] public FloatReference StoredJumpVelocity;
    
    private float gravity;
    private float jumpVelocity;
    
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }
    
    public override void Enter()
    {
        base.Enter();
        playerController.UngroundMotor();
        StoredJumpVelocity.Value = jumpVelocity;
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void ExecuteFixed()
    {
        base.ExecuteFixed();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
