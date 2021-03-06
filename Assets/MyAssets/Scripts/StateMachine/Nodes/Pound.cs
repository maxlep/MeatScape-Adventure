﻿using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class Pound : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference PoundSpeed;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private Vector3Reference NewVelocityOut;


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
        
        currentVelocity = Vector3.down * PoundSpeed.Value;

        NewVelocityOut.Value = currentVelocity;
    }
}
