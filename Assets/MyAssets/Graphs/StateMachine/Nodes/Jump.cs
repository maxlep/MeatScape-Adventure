using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;


public class Jump : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference timeToJumpApex;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference maxJumpHeight;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference StoredJumpVelocity;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private TriggerVariable triggerJumpAnim;
    
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
        triggerJumpAnim.Activate();
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
