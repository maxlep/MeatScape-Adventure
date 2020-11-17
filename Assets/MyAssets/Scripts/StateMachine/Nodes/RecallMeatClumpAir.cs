using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class RecallMeatClumpAir : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [Required] 
    [SerializeField] private IntReference PlayerRecallAttempts;

    public override void Enter()
    {
        base.Enter();
        PlayerRecallAttempts.Value--;
        playerController.RecallClumpToOrbit();
    }
}
