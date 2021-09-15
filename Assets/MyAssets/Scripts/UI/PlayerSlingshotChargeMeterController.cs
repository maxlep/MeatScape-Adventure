using System;
using System.Numerics;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Shapes;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

[ExecuteAlways]
public class PlayerSlingshotChargeMeterController : SerializedMonoBehaviour
{
    [SerializeField] private TransformSceneReference PlayerTransform;
    
    [SerializeField] private GameEvent OnPostRender;

    [TitleGroup("Inputs")] [SerializeField]
    private FloatValueReference PercentToMaxCharge;
    
    [TitleGroup("Inputs")] [SerializeField]
    private TimerVariable TimeToMaxCharge;
    
    [TitleGroup("Inputs")] [SerializeField]
    private FloatValueReference OptimalChargeTime;
    
    [TitleGroup("Inputs")] [SerializeField]
    private FloatValueReference OptimalChargeErrorThreshold;

    [TitleGroup("Style")] [SerializeField] private float ChargeMeterWidth = 15f;
    [TitleGroup("Style")] [SerializeField] private float ChargeMeterHeight = 2f;
    [TitleGroup("Style")] [SerializeField] private float BorderVerticalMargin = .5f;
    [TitleGroup("Style")] [SerializeField] private float BorderHorizontalMargin = .5f;
    [TitleGroup("Style")] [SerializeField] private float FullChargeWidth = .5f;

    [SerializeField] private Rectangle BorderRect;
    [SerializeField] private Rectangle FillRect;
    [SerializeField] private RectTransform TickTransform;
    [SerializeField] private RectTransform OptimalChargeTransform;
    [SerializeField] private Rectangle OptimalChargeRect;
    [SerializeField] private RectTransform FullChargeTransform;
    [SerializeField] private Rectangle FullChargeRect;
    [SerializeField] private RectTransform ScalePivot;
    [SerializeField] private float AppearAnimDuration = .1f;
    [SerializeField] private float TickScaleAnimDuration = .1f;
    [SerializeField] private float TickScaleAnimFactor = 2f;
    [SerializeField] private AnimationCurve TickScaleAnimCurve;

    private LTDescr appearTween;
    private LTDescr disappearTween;
    private LTDescr tickScaleTween;

    private void OnRenderObject() => DrawBar();

    private void Awake()
    {
        if (Application.isPlaying)
            ScalePivot.localScale = Vector3.zero;
    }
    
    public void EnableBar()
    {
        //Cancel the disableBar tweens before starting appear tweens
        if (disappearTween != null) LeanTween.cancel(disappearTween.id);
        if (tickScaleTween != null)
        {
            LeanTween.cancel(tickScaleTween.id);
            TickTransform.localScale = Vector3.one;
        }
        
        //Tween for pop-in when enable bar
        appearTween = LeanTween.value(0f, 1f, AppearAnimDuration).setOnUpdate(t =>
        {
            ScalePivot.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
        });
    }
    
    public void DisableBar()
    {
        if (appearTween != null) LeanTween.cancel(appearTween.id);
        
        //Scale tick up and back down to emphasis the ending location
        tickScaleTween = LeanTween.value(0f, TickScaleAnimFactor, TickScaleAnimDuration).setOnUpdate(t =>
        {
            TickTransform.localScale = new Vector3(1f, t, 1f);
        });
        tickScaleTween.setEase(TickScaleAnimCurve);
        
        //Start disappear tween when tick is done animating
        tickScaleTween.setOnComplete(_ =>
        {
            disappearTween = LeanTween.value(0f, 1f, AppearAnimDuration).setOnUpdate(t =>
            {
                ScalePivot.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            });
        });


    }


    private void DrawBar()
    {
        //Vector3 origin = Offset;
        Vector3 origin = Vector3.zero;
        float halfWidth = ChargeMeterWidth / 2f;
        float heightWithMargin = ChargeMeterHeight - BorderVerticalMargin;
        float widthWithMargin = ChargeMeterWidth - BorderHorizontalMargin;
        Vector3 leftPivot = origin + Vector3.left * halfWidth;
        
        //Border
        BorderRect.Width = ChargeMeterWidth;
        BorderRect.Height = ChargeMeterHeight;
        
        //Fill
        FillRect.Width = widthWithMargin;
        FillRect.Height = heightWithMargin;
        

        //Meter Optimal Timing Fill
        float optimalFillCenterPercent = OptimalChargeTime.Value / TimeToMaxCharge.Duration;
        float optimalFillWidthPercent = OptimalChargeErrorThreshold.Value * 2f / TimeToMaxCharge.Duration;
        float optimalFillCenterX = Mathf.Lerp(0, ChargeMeterWidth, optimalFillCenterPercent);
        float optimalFillWidth = Mathf.Lerp(0, ChargeMeterWidth, optimalFillWidthPercent);
        Vector3 optimalFillPos = leftPivot + Vector3.right * optimalFillCenterX;
        OptimalChargeTransform.localPosition = optimalFillPos;
        OptimalChargeRect.Width = optimalFillWidth;
        OptimalChargeRect.Height = heightWithMargin;
        
        //Meter Full Charge Fill
        Vector3 fullChargeFillPos = leftPivot + Vector3.right * (ChargeMeterWidth - FullChargeWidth/2f);
        FullChargeTransform.localPosition = fullChargeFillPos;
        FullChargeRect.Width = FullChargeWidth;
        FullChargeRect.Height = heightWithMargin;
        
        //Tick Mark Fill
        Vector3 tickMarkPos = leftPivot + Vector3.right * PercentToMaxCharge.Value * ChargeMeterWidth;
        TickTransform.localPosition = tickMarkPos;
    }

    
}
