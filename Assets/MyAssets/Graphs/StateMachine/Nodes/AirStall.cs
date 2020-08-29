using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AirStall : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] [SerializeField] private bool restrictX = true;
    [FoldoutGroup("")] [LabelWidth(120)] [SerializeField] private bool restrictY = true;
    [FoldoutGroup("")] [LabelWidth(120)] [SerializeField] private bool restrictZ = true;
    [FoldoutGroup("")] [LabelWidth(120)] [SerializeField] private Vector3Reference NewVelocityOut;


    public override void Enter()
    {
        base.Enter();
        playerController.onStartUpdateVelocity += UpdateVelocity;
    }
    
    private void UpdateVelocity(Vector3 currentVelocity)
    {
        if (restrictX) currentVelocity.x = 0f;
        if (restrictY) currentVelocity.y = 0f;
        if (restrictZ) currentVelocity.z = 0f;

        NewVelocityOut.Value = currentVelocity;
    }
}
