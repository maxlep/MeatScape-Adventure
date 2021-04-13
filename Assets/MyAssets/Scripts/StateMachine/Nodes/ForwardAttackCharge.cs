using System.Collections;
using System.Collections.Generic;
using MudBun;
using MyAssets.ScriptableObjects.Events;
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
    [TabGroup("Inputs")] [Required]
    private TimerVariable ThrowMinChargeTime;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private FloatReference ThrowSpeedOut;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private FloatReference RunSpeedChargeClumpFactorOut;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private BoolReference ClumpOverChargedOut;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private GameEvent ClumpThrowMaxChargeEvent;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private GameEvent ClumpThrowMinChargeEvent;
    
    
    
    

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
    }

    public override void Execute()
    {
        base.Execute();
        float elapsedChargeTime = Time.time - chargeStartTime;

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
        ClumpOverChargedOut.Value = reachedMaxCharge;
        RunSpeedChargeClumpFactorOut.Value = 1f;
    }
}
