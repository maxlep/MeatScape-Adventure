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

    [TitleGroup("Style")] [SerializeField] private Vector3 Offset = Vector3.zero;
    [TitleGroup("Style")] [SerializeField] private float ChargeMeterWidth = 15f;
    [TitleGroup("Style")] [SerializeField] private float ChargeMeterHeight = 2f;
    [TitleGroup("Style")] [SerializeField] private float CornerRadius = .5f;
    [TitleGroup("Style")] [SerializeField] private float BorderThickness = .3f;
    [TitleGroup("Style")] [SerializeField] private float BorderVerticalMargin = .05f;
    [TitleGroup("Style")] [SerializeField] private float BorderHorizontalMargin = .05f;
    [TitleGroup("Style")] [SerializeField] private Color FillColor = Color.black;
    [TitleGroup("Style")] [SerializeField] private float FillThickness = 1f;
    [TitleGroup("Style")] [SerializeField] private float TickWidth = .5f;
    [TitleGroup("Style")] [SerializeField] private float TickHeight = 3f;
    [TitleGroup("Style")] [SerializeField] private float TickThickness = .2f;
    [TitleGroup("Style")] [SerializeField] private Color TickBorderColor = Color.white;
    [TitleGroup("Style")] [SerializeField] private Color TickFillColor = Color.white;
    [TitleGroup("Style")] [SerializeField] private Color OptimalFillColor = Color.green;
    [TitleGroup("Style")] [SerializeField] private float FullChargeWidth = .5f;
    [TitleGroup("Style")] [SerializeField] private Color FullChargeColor = Color.yellow;

    [SerializeField] private RectTransform tickTransform;
    [SerializeField] private Rectangle tickRect;


    //private void OnEnable() => OnPostRender.Subscribe(DrawBar);
    //private void OnDisable() => OnPostRender.Subscribe(DrawBar);

    private void OnRenderObject() => DrawBar();


    private void DrawBar()
    {
        Draw.Matrix = transform.localToWorldMatrix;
        Draw.ZTest = CompareFunction.Always;
        Draw.BlendMode = ShapesBlendMode.Transparent;
        Draw.LineGeometry = LineGeometry.Flat2D;
        Draw.LineThickness = 1f;

        Vector3 origin = Offset;
        float halfWidth = ChargeMeterWidth / 2f;
        float widthWithMargin = ChargeMeterWidth - BorderHorizontalMargin;
        float heightWithMargin = ChargeMeterHeight - BorderVerticalMargin;
        Vector3 leftPivot = origin + Vector3.left * halfWidth;
        
        //Background
        //Draw.Line(origin -Vector3.right * halfWidth, origin + Vector3.right * halfWidth, LineEndCap.None, Color.black);
        //Draw.Rectangle(ShapesBlendMode.Transparent, true, origin, Quaternion.identity, new Rect(-halfWidth, 0f, chargeMeterWidth, chargeMeterHeight), Color.black, 1f, Vector4.one);
        //Draw.RectangleBorder(origin, Quaternion.identity, new Rect(-halfWidth, 0f, chargeMeterWidth, chargeMeterHeight), Color.black, 1f, Vector4.one);
        
        //Meter Fill
        Draw.RectangleBorder(origin, quaternion.identity, new Vector2(widthWithMargin, 
            heightWithMargin), RectPivot.Center, FillThickness, CornerRadius, FillColor);
        
        //Meter Optimal Timing Fill
        float optimalFillCenterPercent = OptimalChargeTime.Value / TimeToMaxCharge.Duration;
        float optimalFillWidthPercent = OptimalChargeErrorThreshold.Value * 2f / TimeToMaxCharge.Duration;
        float optimalFillCenterX = Mathf.Lerp(0, ChargeMeterWidth, optimalFillCenterPercent);
        float optimalFillWidth = Mathf.Lerp(0, ChargeMeterWidth, optimalFillWidthPercent);
        Vector3 optimalFillPos = leftPivot + Vector3.right * optimalFillCenterX;
        Draw.RectangleBorder(optimalFillPos, quaternion.identity, new Vector2(optimalFillWidth, 
            heightWithMargin), RectPivot.Center, FillThickness, CornerRadius, OptimalFillColor);
        
        //Meter Full Charge Fill
        Vector3 fullChargeFillPos = leftPivot + Vector3.right * (ChargeMeterWidth - FullChargeWidth/2f);
        Draw.RectangleBorder(fullChargeFillPos, quaternion.identity, new Vector2(FullChargeWidth, 
            heightWithMargin), RectPivot.Center, FillThickness, CornerRadius, FullChargeColor);
        
        //Meter Border
        Draw.RectangleBorder(origin, quaternion.identity, new Vector2(ChargeMeterWidth, 
            ChargeMeterHeight), RectPivot.Center, BorderThickness, CornerRadius, Color.black);
        
        //Tick Mark Fill
        float tickHeightWithMargin = TickHeight - BorderVerticalMargin;
        float tickWidthWithMargin = TickWidth - BorderHorizontalMargin;
        Vector3 tickMarkPos = leftPivot + Vector3.right * PercentToMaxCharge.Value * ChargeMeterWidth;
        Draw.RectangleBorder(tickMarkPos, quaternion.identity, new Vector2(tickWidthWithMargin, 
            tickHeightWithMargin), RectPivot.Center, FillThickness, CornerRadius, TickFillColor);

        tickTransform.localPosition = tickMarkPos;

        //Tick Mark Border
        Draw.RectangleBorder(tickMarkPos, quaternion.identity, new Vector2(TickWidth, 
            TickHeight), RectPivot.Center, TickThickness, CornerRadius, TickBorderColor);

        //Reset global Draw parameters
        Draw.Matrix = Matrix4x4.identity;
    }
}
