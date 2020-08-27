using Sirenix.OdinInspector;
using UnityEngine;


public class Jump : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] public FloatReference timeToJumpApex;
    [FoldoutGroup("")] [LabelWidth(120)] public FloatReference maxJumpHeight;
    [FoldoutGroup("")] [LabelWidth(120)] public FloatReference StoredJumpVelocity;
    
    private float gravity;
    private float jumpVelocity;
    
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
        gravity = -(2 * maxJumpHeight.Value) / Mathf.Pow(timeToJumpApex.Value, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex.Value;
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
