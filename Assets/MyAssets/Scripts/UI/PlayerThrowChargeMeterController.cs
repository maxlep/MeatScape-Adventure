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
        private float fillLastUpdate;
        [SerializeField, Range(0, 1)] private float appearPct;

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

        public Color backgroundFillColor;
        public Color outlineColor;
        public Color minFillColor;
        public Color fillColor;
        [FormerlySerializedAs("fillFlashColor")] public Color flashColor;
        public Color fullFlashColor;

        public AnimationCurve lerpColor;
        [ShowInInspector] private float elapsedAlternateTime;
        [OnValueChanged("ResetFlashTime")] public float chargingFlashDelay;
        [OnValueChanged("ResetFlashTime")] public float fullFlashDelay;

        public bool released = false;
        public float releasedElapsedTime;
        public float releasedAnimTime = 0.2f;
        public AnimationCurve releasedAnimCurve;
        
        public bool popped = false;
        public bool complete = false;
        public float poppedReleasedOverlapTime;
        [FormerlySerializedAs("poppoedElapsedTime")] public float poppedElapsedTime;
        public float poppedAnimTime;
        public AnimationCurve poppedAnimCurve;
        
        public float popupTime;
        public float elapsedPopupTime;
        public float originalBarRadius;
        public float originalBarThickness;

        public bool isReset = true;

        public bool flipFillDirection = false;

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

        private void OnEnable()
        {
            appearPct = 0;
        }

#if UNITY_EDITOR
        private void Start()
        {
            if (Application.isPlaying)
            {
                appearPct = 0;
                elapsedPopupTime = 0;
                releasedElapsedTime = 0;
                poppedElapsedTime = 0;
                released = false;
                popped = false;
                complete = false;
                isReset = true;
            }
        }
#else
        private void Start()
        {
            appearPct = 0;
            elapsedPopupTime = 0;
            releasedElapsedTime = 0;
            poppedElapsedTime = 0;
            released = false;
            popped = false;
            complete = false;
            isReset = true;
        }
#endif

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
        
        
#region Interface

        public void Release()
        {
            released = true;
        }

#endregion

#region Drawing

        private void DrawBar(UnityEngine.Camera cam)
        {
            #if UNITY_EDITOR
            var tempFill = Application.isPlaying ? _fillPct.Value : fillPct;
            var deltaTime = Application.isPlaying ? Time.unscaledDeltaTime : Time.fixedUnscaledDeltaTime;
            #else
            var tempFill = _fillPct.Value;
            var deltaTime = Time.unscaledDeltaTime;
            #endif
            
            if (released && !isReset)
            {
                tempFill = fillLastUpdate;
            }
            
            fillLastUpdate = tempFill;
            
            if (Mathf.Approximately(tempFill, 0) && Mathf.Approximately(appearPct, 0))
            {
                return;
            }
            
            if (tempFill > 0 && appearPct < 1 && !released)
            {
                if (isReset) isReset = false; 
                elapsedPopupTime += deltaTime;
                appearPct = elapsedPopupTime / popupTime;
                if (appearPct >= 1)
                {
                    appearPct = 1;
                    elapsedPopupTime = 0;
                }
            }
            
            if ((Mathf.Approximately(tempFill, 0) || complete) && appearPct > 0)
            {
                elapsedPopupTime += deltaTime;
                appearPct = 1 - (elapsedPopupTime / popupTime);
                if (appearPct <= 0)
                {
                    appearPct = 0;
                    elapsedPopupTime = 0;
                    releasedElapsedTime = 0;
                    poppedElapsedTime = 0;
                    released = false;
                    popped = false;
                    complete = false;
                    isReset = true;
                }
            }
            
            var uiPos = arcPosUI;// + UnityEngine.Camera.main.WorldToScreenPoint(follow?.position ?? Vector3.zero).xz();
            
            bool minFilled = tempFill >= minFillThreshold.Value;
            bool maxFilled = tempFill >= maxFillThreshold.Value; 
            float angRadMin = barAngularSpanRad / 2 + ShapesMath.TAU / 2;
            float angRadMax = -barAngularSpanRad / 2 + ShapesMath.TAU / 2;
            float outerRadius = barRadius * appearPct + barThickness / 2 * appearPct;
            if (flipFillDirection)
            {
                var temp = angRadMin;
                angRadMin = angRadMax;
                angRadMax = temp;
            }

            float chargeAngRad = Mathf.Lerp(angRadMin, angRadMax, tempFill);
            float minChargeAngRad = angRadMin;

            if (released)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    fillPct = 0;
                }
                #endif
                
                minFilled = true;
                if (releasedElapsedTime < releasedAnimTime)
                {
                    releasedElapsedTime += deltaTime;
                    releasedElapsedTime = Mathf.Min(releasedElapsedTime + deltaTime, releasedAnimTime);
                    if (releasedElapsedTime >= releasedAnimTime - poppedReleasedOverlapTime)
                    {
                        popped = true;
                    }
                    // if (Mathf.Approximately(releasedElapsedTime, releasedAnimTime))
                    // {
                    //     popped = true;
                    // }
                }

                var releasedPct = Mathf.Clamp01(releasedElapsedTime / releasedAnimTime);
                var releasedAnimFactor = releasedAnimCurve.Evaluate(releasedPct);
                minChargeAngRad = Mathf.Lerp(minChargeAngRad, chargeAngRad, releasedAnimFactor);
            }
            
            // Background Fill
            Draw.Color = backgroundFillColor;
            DrawArcUI(_rectTransform, uiPos, barRadius * appearPct, barThickness * appearPct, angRadMin, angRadMax);
            Vector2 pTop = uiPos + ShapesMath.AngToDir(angRadMax) * barRadius * appearPct;
            Vector2 pBot = uiPos + ShapesMath.AngToDir(angRadMin) * barRadius * appearPct;
            DrawDiscUI(_rectTransform, pTop, barThickness / 2f * appearPct);
            DrawDiscUI(_rectTransform, pBot, barThickness / 2f * appearPct);
            
            // Outline
            Draw.Color = outlineColor;
            if (flipFillDirection)
            {
                DrawRoundedArcOutline(_rectTransform, uiPos, barRadius * appearPct, barThickness * appearPct, barOutlineThickness * appearPct, angRadMin, angRadMax);
            }
            else
            {
                DrawRoundedArcOutline(_rectTransform, uiPos, barRadius * appearPct, barThickness * appearPct, barOutlineThickness * appearPct, angRadMax, angRadMin);
            }
            
            // Ghost Outline
            // Draw.Color = outlineColor;
            // DrawArcUI(_rectTransform, arcPosUI, barRadius, barThickness / 2, angRadMin, angRadMax);
            // DrawDiscUI(_rectTransform, );

            // Main fill
            
            if (minFilled)
            {
                var flashDelay = (tempFill >= maxFillThreshold.Value ? fullFlashDelay : chargingFlashDelay);
                elapsedAlternateTime += deltaTime;
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
            DrawArcUI(_rectTransform, uiPos, barRadius * appearPct, barThickness * appearPct, minChargeAngRad, chargeAngRad);
            
            // End fill
            Vector2 movingTopPos = uiPos + ShapesMath.AngToDir(chargeAngRad) * barRadius * appearPct;
            Vector2 bottomPos = uiPos + ShapesMath.AngToDir(minChargeAngRad) * barRadius * appearPct;
            var circleRadius = barThickness / 2f * appearPct;
            
            if (popped)
            {
                if (minFilled)
                {
                    poppedElapsedTime += deltaTime;
                    poppedElapsedTime = Mathf.Min(poppedElapsedTime + deltaTime, poppedAnimTime);
                    if (Mathf.Approximately(poppedElapsedTime, poppedAnimTime) && !complete)
                    {
                        complete = true;
                    }

                    var pct = poppedElapsedTime / poppedAnimTime;
                    var lerp = 1 + poppedAnimCurve.Evaluate(pct);
                    circleRadius *= lerp;
                }
                else
                {
                    complete = true;
                }
            }
            // Bottom
            DrawDiscUI(_rectTransform, bottomPos, circleRadius);
            // Top
            DrawDiscUI(_rectTransform, movingTopPos, circleRadius);// + barOutlineThickness / 2f);
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
                ((Vector3)offset + rectTransform.position).RotatePointAroundPivot(rectTransform.position, rectTransform.rotation);
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
    }
}