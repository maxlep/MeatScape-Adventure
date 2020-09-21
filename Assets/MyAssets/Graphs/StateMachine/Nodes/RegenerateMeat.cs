using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.ScriptableObjects.Variables;

public class RegenerateMeat : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference regenerateMeatTime;
    
    private float regenerateMeatStartTime;

    public override void Enter()
    {
        base.Enter();
        regenerateMeatStartTime = Time.time;
    }

    public override void Execute()
    {
        base.Execute();
        if(Time.time >= (regenerateMeatStartTime + regenerateMeatTime.Value)) {
            playerController.CurrentSize++;
            regenerateMeatStartTime = Time.time;
        }
    }
}
