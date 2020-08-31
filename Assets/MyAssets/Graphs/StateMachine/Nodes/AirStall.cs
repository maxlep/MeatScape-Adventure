using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class AirStall : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private bool restrictX = true;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private bool restrictY = true;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private bool restrictZ = true;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private Vector3Reference NewVelocityOut;


    public override void Enter()
    {
        base.Enter();
        playerController.onStartUpdateVelocity += UpdateVelocity;
    }

    public override void Exit()
    {
        base.Exit();
        playerController.onStartUpdateVelocity -= UpdateVelocity;
    }

    private void UpdateVelocity(Vector3 currentVelocity)
    {
        if (restrictX) currentVelocity.x = 0f;
        if (restrictY) currentVelocity.y = 0f;
        if (restrictZ) currentVelocity.z = 0f;

        NewVelocityOut.Value = currentVelocity;
    }
}
