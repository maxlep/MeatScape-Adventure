using System.Collections;
using System.Collections.Generic;
using MudBun;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class ForwardAttackCharge : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference MinThrowSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference MaxThrowSpeed;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference TimeToMaxCharge;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference RunSpeedChargeClumpFactor;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private FloatReference ThrowSpeedOut;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private FloatReference RunSpeedChargeClumpFactorOut;
    


    private float chargeStartTime = Mathf.NegativeInfinity;
    
    public override void Enter()
    {
        base.Enter();
        chargeStartTime = Time.time;
        RunSpeedChargeClumpFactorOut.Value = RunSpeedChargeClumpFactor.Value;
    }

    public override void Exit()
    {
        base.Exit();
        float chargedTime = Time.time - chargeStartTime;
        float percentToMaxCharge = Mathf.Lerp(0f, TimeToMaxCharge.Value, chargedTime);
        ThrowSpeedOut.Value = Mathf.Lerp(MinThrowSpeed.Value, MaxThrowSpeed.Value, percentToMaxCharge);
        RunSpeedChargeClumpFactorOut.Value = 1f;
    }
}
