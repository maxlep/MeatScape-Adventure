using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class AirStall : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private bool restrictX = true;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private bool restrictY = true;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private bool restrictZ = true;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private Vector3Reference NewVelocityOut;


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

    private void UpdateVelocity(VelocityInfo velocityInfo)
    {
        Vector3 currentVelocity = velocityInfo.currentVelocity;
        
        if (restrictX) currentVelocity.x = 0f;
        if (restrictY) currentVelocity.y = 0f;
        if (restrictZ) currentVelocity.z = 0f;

        NewVelocityOut.Value = currentVelocity;
    }
}
