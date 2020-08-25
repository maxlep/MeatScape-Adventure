using UnityEngine;


public class Jump : PlayerStateNode
{
    public float timeToJumpApex = .4f;
    public float maxJumpHeight = 4f;
    
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
        playerController.UngroundMotor();
        playerController.SetJumpVelocity(jumpVelocity);
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
