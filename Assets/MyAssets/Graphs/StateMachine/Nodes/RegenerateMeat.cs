using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.ScriptableObjects.Variables;

public class RegenerateMeat : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference RegenerateMeatTime;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference MoveSpeed;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference MoveSpeedDecreaseFactor;
    
    private float regenerateMeatStartTime;
    private float realMoveSpeed;

    public override void Enter()
    {
        base.Enter();
        regenerateMeatStartTime = Time.time;
        realMoveSpeed = MoveSpeed.Value;
        MoveSpeed.Value /= MoveSpeedDecreaseFactor.Value;
    }

    public override void Execute()
    {
        base.Execute();
        if(Time.time >= (regenerateMeatStartTime + RegenerateMeatTime.Value)) {
            playerController.CurrentSize++;
            regenerateMeatStartTime = Time.time;
            realMoveSpeed = MoveSpeed.Value;
            MoveSpeed.Value /= MoveSpeedDecreaseFactor.Value;
        }
    }

    public override void Exit()
    {
        base.Exit();
        MoveSpeed.Value = realMoveSpeed;
    }
}
