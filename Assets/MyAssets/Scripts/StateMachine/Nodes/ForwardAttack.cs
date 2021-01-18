using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.Scripts.Utils;
using Sirenix.Utilities;

public class ForwardAttack : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private MeatClumpController MeatClumpPrefab;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference ThrowDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference ThrowSpeed;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private BoolReference WaitedAttackDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TransformSceneReference ThrowPoint;
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
        var throwDirection = autoAimTarget.SafeIsUnityNull()
            ? GetMoveDirection().normalized
            : (autoAimTarget.position - playerController.transform.position).normalized;

        if(Mathf.Approximately(throwDirection.magnitude, 0)) throwDirection = ThrowPoint.Value.forward;

        MeatClumpController clump = Instantiate(MeatClumpPrefab);
        clump.transform.position = ThrowPoint.Value.position;
        clump.SetMoving(ThrowSpeed.Value, throwDirection);

        playerController.OnClumpThrown(throwDirection);
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

        WaitedAttackDelay.Value = false;
        
        TimeUtils.SetTimeout(ThrowDelay.Value, () => WaitedAttackDelay.Value = true);
    }
}