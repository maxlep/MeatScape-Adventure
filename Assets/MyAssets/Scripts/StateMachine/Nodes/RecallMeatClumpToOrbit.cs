using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class RecallMeatClumpToOrbit : PlayerStateNode
{

    public override void Enter()
    {
        base.Enter();
        playerController.RecallClumpToOrbit();
    }
}
