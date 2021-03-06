using System.Collections;
using System.Collections.Generic;
using MudBun;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

public class ForwardAttackCharge : PlayerStateNode
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference MinThrowSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference MaxThrowSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatValueReference ThrowSpeedStatMultiplier;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference TimeToMaxCharge;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference RunSpeedChargeClumpFactor;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private TimerVariable ThrowMinChargeTime;

    #endregion

    #region Outputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private FloatReference ThrowSpeedOut;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private FloatReference RunSpeedChargeClumpFactorOut;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private FloatReference ClumpThrowPercentToMaxCharge;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private FloatReference ClumpMinChargePercent;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private TimerVariable ClumpMinChargeTime;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private BoolReference ClumpOverChargedOut;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private GameEvent ClumpThrowMaxChargeEvent;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private GameEvent ClumpThrowMinChargeEvent;

    #endregion

    private float chargeStartTime = Mathf.NegativeInfinity;
    private bool reachedMinCharge;
    private bool reachedMaxCharge;

    public override void Enter()
    {
        base.Enter();
        reachedMinCharge = false;
        reachedMaxCharge = false;
        chargeStartTime = Time.time;
        RunSpeedChargeClumpFactorOut.Value = RunSpeedChargeClumpFactor.Value;
        ClumpMinChargePercent.Value =  ClumpMinChargeTime.Duration / TimeToMaxCharge.Value;
    }

    public override void Execute()
    {
        base.Execute();
        float elapsedChargeTime = Time.time - chargeStartTime;
        ClumpThrowPercentToMaxCharge.Value = Mathf.InverseLerp(0f, TimeToMaxCharge.Value, elapsedChargeTime);

        if (!reachedMaxCharge && elapsedChargeTime > TimeToMaxCharge.Value)
        {
            reachedMaxCharge = true;
            ClumpThrowMaxChargeEvent.Raise();
        }

        if (!reachedMinCharge && elapsedChargeTime > ThrowMinChargeTime.Duration)
        {
            reachedMinCharge = true;
            ClumpThrowMinChargeEvent.Raise();
        }
            
    }

    public override void Exit()
    {
        base.Exit();
        //float chargedTime = Time.time - chargeStartTime;
        //float percentToMaxCharge = Mathf.Lerp(0f, TimeToMaxCharge.Value, chargedTime);
        //ThrowSpeedOut.Value = Mathf.Lerp(MinThrowSpeed.Value, MaxThrowSpeed.Value, percentToMaxCharge);
        ThrowSpeedOut.Value = (reachedMaxCharge) ? MaxThrowSpeed.Value : MinThrowSpeed.Value;
        ThrowSpeedOut.Value *= ThrowSpeedStatMultiplier.Value;
        ClumpOverChargedOut.Value = reachedMaxCharge;
        RunSpeedChargeClumpFactorOut.Value = 1f;
        ClumpThrowPercentToMaxCharge.Value = 0;
    }
}
