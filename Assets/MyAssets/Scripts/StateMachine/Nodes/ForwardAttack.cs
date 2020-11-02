using UnityEngine.InputSystem;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.Scripts.Utils;
using Sirenix.Utilities;

public class ForwardAttack : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference throwDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference throwSpeed;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private BoolReference waitedAttackDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TransformSceneReference firePoint;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TransformSceneReference PlayerCameraTransform;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private Vector2Reference MoveInput;

    private bool clumpThrown;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    private Vector3 GetMoveDirection()
    {
        Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        return new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }
    
    public override void Enter()
    {
        base.Enter();

        var autoAimTarget = playerController.AimTarget;
        var fireDirection = autoAimTarget.SafeIsUnityNull()
            ? GetMoveDirection().normalized
            : (autoAimTarget.position - playerController.transform.position).normalized;

        if(Mathf.Approximately(fireDirection.magnitude, 0)) fireDirection = firePoint.Value.forward;

        MeatClumpController clump = playerController.DetachClump(fireDirection);
        clump.transform.position = firePoint.Value.position;
        clump.SetMoving(throwSpeed.Value, fireDirection);
        
        if(!playerController.unlimitedClumps) playerController.CurrentSize -= 1;
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

        waitedAttackDelay.Value = false;
        
        LeanTween.value(0f, 1f, throwDelay.Value)
            .setOnComplete(_ =>
            {
                waitedAttackDelay.Value = true;
            });
    }
}