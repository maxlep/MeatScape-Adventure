﻿using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class Pound : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference PoundSpeed;
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
        currentVelocity = Vector3.down * PoundSpeed.Value;

        NewVelocityOut.Value = currentVelocity;
    }
}