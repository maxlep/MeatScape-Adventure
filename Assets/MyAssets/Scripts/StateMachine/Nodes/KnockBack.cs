using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;


public class KnockBack : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected Vector3Reference KnockbackForce;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private BoolVariable SlingshotReady;
    

    public override void Enter()
    {
        base.Enter();
        playerController.AddImpulseOverlayed(KnockbackForce.Value, true);
        SlingshotReady.Value = true;
    }
}
