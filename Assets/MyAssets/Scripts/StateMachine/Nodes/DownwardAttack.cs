using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.Scripts.Utils;

public class DownwardAttack : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private MeatClumpController MeatClumpPrefab;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference ThrowDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference ThrowSpeed;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private BoolReference WaitedAttackDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TransformSceneReference ThrowPoint;
    
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }
    
    public override void Enter()
    {
        base.Enter();

        var throwDirection = -ThrowPoint.Value.up;
        MeatClumpController clump = Instantiate(MeatClumpPrefab);
        clump.transform.position = ThrowPoint.Value.position;
        clump.SetMoving(ThrowSpeed.Value, throwDirection);

        playerController.OnClumpThrown(throwDirection, true);
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