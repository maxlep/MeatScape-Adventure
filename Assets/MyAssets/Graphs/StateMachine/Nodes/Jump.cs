using System;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;


public class Jump : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference timeToJumpApex;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference maxJumpHeight;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference StoredJumpVelocity;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference jumpGroundDelay;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private BoolReference hasWaitedJumpDelay;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private TriggerVariable triggerJumpAnim;

    private float gravity;
    private float jumpVelocity;
    
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    public override void RuntimeInitialize()
    {
        base.RuntimeInitialize();
    }

    public override void Enter()
    {
        base.Enter();
        gravity = -(2 * maxJumpHeight.Value) / Mathf.Pow(timeToJumpApex.Value, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex.Value;
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
        hasWaitedJumpDelay.Value = false;
        
        LeanTween.value(0f, 1f, jumpGroundDelay.Value)
            .setOnComplete(_ =>
            {
                hasWaitedJumpDelay.Value = true;
            });
    }
}
