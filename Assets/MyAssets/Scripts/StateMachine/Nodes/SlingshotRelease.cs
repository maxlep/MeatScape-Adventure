using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class SlingshotRelease : PlayerStateNode
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference MaxForce;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference OptimalChargeMultiplier;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private Vector3Reference AccumulatedSlingshotForce;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private TimerVariable DelayTimer;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private BoolVariable SlingshotReady;

    #endregion

    private bool isOptimalRelease;
    
    
    public override void Enter()
    {
        base.Enter();
        Release();
        SlingshotReady.Value = false;
    }

    private void Release()
    {
        if (isOptimalRelease)
        {
            AccumulatedSlingshotForce.Value = MaxForce.Value * OptimalChargeMultiplier.Value * AccumulatedSlingshotForce.Value.normalized;
        }

        playerController.AddImpulseOverlayed(AccumulatedSlingshotForce.Value);
        playerController.ToggleArrow(false);
        DelayTimer.StartTimer();
    }

    //For use by transition
    public void SetOptimalRelease(bool isOptimal)
    {
        isOptimalRelease = isOptimal;
    }
}
