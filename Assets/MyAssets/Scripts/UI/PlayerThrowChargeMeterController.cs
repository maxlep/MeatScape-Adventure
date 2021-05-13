using System;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MyAssets.Scripts.UI
{
    [ExecuteAlways]
    public class PlayerThrowChargeMeterController : SerializedMonoBehaviour
    {
        
#region Inspector
        
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _rectTransform;
        
        [SerializeField] private MyAssets.ScriptableObjects.Variables.ValueReferences.FloatValueReference _fillPct;
        [SerializeField, Range(0, 1)] private float fillPct;

        [SerializeField] private FloatValueReference minFillThreshold;
        [SerializeField] private FloatValueReference maxFillThreshold;

        public float barRadius = 20;
        public float barThickness = 10;
        public float barOutlineThickness = 10;
        public float barAngularSpanRad = 1;

        public float tickSizeSmol = 0.1f;
        public float tickSizeLorge = 0.1f;
        public float tickTickness;
        public float fontSize = 0.1f;
        public float fontSizeLorge = 0.1f;
        public float percentLabelOffset = 0.1f;
        public float fontGrowRangePrev = 0.1f;
        public float fontGrowRangeNext = 0.1f;

        public Vector2 arcPosUI;
        
        public Color outlineColor;
        public Color minFillColor;
        public Color fillColor;
        [FormerlySerializedAs("fillFlashColor")] public Color flashColor;
        public Color fullFlashColor;

        public AnimationCurve lerpColor;
        [ShowInInspector] private float elapsedAlternateTime;
        [OnValueChanged("ResetFlashTime")] public float chargingFlashDelay;
        [OnValueChanged("ResetFlashTime")] public float fullFlashDelay;

#endregion

#region Lifecycle

        private void OnRenderObject()
        {
            DrawBar(UnityEngine.Camera.main);
        }

        private void ResetFlashTime()
        {
            elapsedAlternateTime = 0;
        }

#endregion

#region Camera

        private static float ScreenToWorldScale(Canvas canvas, float screenScale)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                return screenScale * canvas.planeDistance;
            }

            return screenScale;
        }

#endregion

#region Drawing

        private void DrawBar(UnityEngine.Camera cam)
        {
            #if UNITY_EDITOR
            var tempFill = Application.isPlaying ? _fillPct.Value : fillPct;
            #else
            var tempFill = fillPct;
            #endif

            var uiPos = arcPosUI;// + UnityEngine.Camera.main.WorldToScreenPoint(follow?.position ?? Vector3.zero).xz();
            
            bool minFilled = tempFill >= minFillThreshold.Value;
            bool maxFilled = tempFill >= maxFillThreshold.Value; 
            float angRadMin = barAngularSpanRad / 2 + ShapesMath.TAU / 2;
            float angRadMax = -barAngularSpanRad / 2 + ShapesMath.TAU / 2;
            float outerRadius = barRadius + barThickness / 2;

            float chargeAngRad = Mathf.Lerp(angRadMin, angRadMax, tempFill);
            float minChargeAngRad;
            if (minFilled)
            {
                minChargeAngRad = Mathf.Lerp(angRadMin, angRadMax, minFillThreshold.Value);
            }
            else
            {
                minChargeAngRad = chargeAngRad;
            }
            
            // Outline
            Draw.Color = outlineColor;
            DrawRoundedArcOutline(_rectTransform, uiPos, barRadius, barThickness, barOutlineThickness, angRadMax, angRadMin);
            
            // Ghost Outline
            // Draw.Color = outlineColor;
            // DrawArcUI(_rectTransform, arcPosUI, barRadius, barThickness / 2, angRadMin, angRadMax);
            // DrawDiscUI(_rectTransform, );

            // Main fill
            
            if (minFilled)
            {
                var flashDelay = (tempFill >= maxFillThreshold.Value ? fullFlashDelay : chargingFlashDelay);
                #if UNITY_EDITOR
                elapsedAlternateTime += Application.isPlaying ? Time.unscaledDeltaTime : Time.fixedUnscaledDeltaTime;
                #else
                elapsedAlternateTime += Time.unscaledDeltaTime;
                #endif
                if (elapsedAlternateTime >= flashDelay)
                {
                    elapsedAlternateTime = elapsedAlternateTime % flashDelay;
                }

                var pct = elapsedAlternateTime / flashDelay;
                var lerp = lerpColor.Evaluate(Double.IsNaN(pct) ? 0 : pct);
                // Debug.Log($"{elapsedAlternateTime} {Time.unscaledDeltaTime}");

                var tempFlashColor = maxFilled ? fullFlashColor : flashColor;
                var resultColor = Color.Lerp(fillColor, tempFlashColor, lerp);
                Draw.Color = resultColor;
            }
            else
            {
                Draw.Color = minFillColor;
            }
            DrawArcUI(_rectTransform, uiPos, barRadius, barThickness, minChargeAngRad, chargeAngRad);
            
            // End fill
            Vector2 movingTopPos = uiPos + ShapesMath.AngToDir(chargeAngRad) * barRadius;
            Vector2 bottomPos = uiPos + ShapesMath.AngToDir(minChargeAngRad) * barRadius;
            // Bottom
            DrawDiscUI(_rectTransform, bottomPos, barThickness / 2f);
            // Top
            DrawDiscUI(_rectTransform, movingTopPos, barThickness / 2f);// + barOutlineThickness / 2f);
            // Draw.Color = Color.blue;
            // DrawDiscUI(_rectTransform, bottomPos, barThickness / 2f - barOutlineThickness / 2f);
            
            // Ticks
            Draw.LineEndCaps = LineEndCap.None;
        }

        private static void DrawRoundedArcOutline(RectTransform rectTransform, Vector2 origin, float radius, float thickness,
            float outlineThickness, float angStart, float angEnd)
        {
            // inner / outer
            float innerRadius = radius - thickness / 2;
            float outerRadius = radius + thickness / 2;
            const float aaMargin = 0.01f;
            DrawArcUI(rectTransform, origin, innerRadius, outlineThickness, angStart - aaMargin, angEnd + aaMargin);
            DrawArcUI(rectTransform, origin, outerRadius, outlineThickness, angStart - aaMargin, angEnd + aaMargin);
        
            // rounded caps
            Vector2 originBottom = origin + ShapesMath.AngToDir(angStart) * radius;
            Vector2 originTop = origin + ShapesMath.AngToDir(angEnd) * radius;
            DrawArcUI(rectTransform, originBottom, thickness / 2, outlineThickness, angStart, angStart - ShapesMath.TAU / 2);
            DrawArcUI(rectTransform, originTop, thickness / 2, outlineThickness, angEnd, angEnd + ShapesMath.TAU / 2);
        }

        private static Vector3 UIToWorldPosition(RectTransform rectTransform, Vector2 offset)
        {
            var worldPosition =
                RotatePointAroundPivot(((Vector3)offset + rectTransform.position), rectTransform.position, rectTransform.rotation);
            return worldPosition;
        }

        private static void DrawArcUI(RectTransform rectTransform, Vector2 offset, float radius, float thickness, float angStart, float angEnd)
        {
            var worldPosition = UIToWorldPosition(rectTransform, offset);

            Draw.Arc(worldPosition, rectTransform.rotation, radius, thickness, angStart, angEnd);
        }

        private static void DrawDiscUI(RectTransform rectTransform, Vector2 offset, float radius)
        {
            var worldPosition = UIToWorldPosition(rectTransform, offset);
            
            Draw.Disc(worldPosition, rectTransform.rotation, radius);
        }

#endregion
        
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation) {
            var dir = point - pivot;
            dir = rotation * dir;
            point = dir + pivot;
            return point;
        }
    }
}