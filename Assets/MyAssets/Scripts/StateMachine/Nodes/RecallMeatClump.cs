using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.ScriptableObjects.Variables;

public class RecallMeatClump : PlayerStateNode
{
    public override void Enter()
    {
        base.Enter();
        playerController.RecallClump();
    }
}
