using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.Scripts.Utils;

public class Jump : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference timeToJumpApex;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference maxJumpHeight;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference StoredJumpVelocity;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference jumpGroundDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private BoolReference hasWaitedJumpDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TriggerVariable triggerJumpAnim;

    private float gravity;
    private float jumpVelocity;
    
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    public override void RuntimeInitialize(int startNodeIndex)
    {
        base.RuntimeInitialize(startNodeIndex);
    }

    public override void Enter()
    {
        base.Enter();
        gravity = -(2 * maxJumpHeight.Value) / Mathf.Pow(timeToJumpApex.Value, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex.Value;
        playerController.UngroundMotor();
        StoredJumpVelocity.Value = jumpVelocity + Mathf.Max(playerController.GetPlatformVelocity().y, 0f);
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

        hasWaitedJumpDelay.Value = false;

        TimeUtils.SetTimeout(jumpGroundDelay.Value, () => hasWaitedJumpDelay.Value = true);
    }
}
