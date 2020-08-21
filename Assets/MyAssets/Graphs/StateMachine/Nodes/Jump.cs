using UnityEngine;


public class Jump : PlayerStateNode
{
    public float JumpVelocity = 50f;
    
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }
    
    public override void Enter()
    {
        playerController.UngroundMotor();
        playerController.SetJumpVelocity(JumpVelocity);
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

    private void UpdateVelocity(ref Vector3 currentVelocity)
    {
        if (!isActiveState) return;
        
        currentVelocity.y = JumpVelocity;
    }
}
