using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.ScriptableObjects.Variables;

public class RecallMeatClump : PlayerStateNode
{
    // [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference RegenerateMeatTime;

    public override void Enter()
    {
        base.Enter();
        playerController.RecallClump();
    }
}
