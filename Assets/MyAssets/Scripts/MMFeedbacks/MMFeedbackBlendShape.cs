using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
    /// This feedback will animate the scale of the target object over time when played
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackPath("Renderer/BlendShape")]
    [FeedbackHelp("This feedback will animate the target's blend shape, for the specified duration (in seconds). You can apply a multiplier, that will multiply each animation curve value.")]
    public class MMFeedbackBlendShape : MMFeedback
    {
        /// the possible modes this feedback can operate on
        public enum Modes { Absolute, Additive, ToDestination }
        /// the possible timescales for the animation of the scale
        public enum TimeScales { Scaled, Unscaled }
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        #endif

        [Header("Scale")]
        /// the mode this feedback should operate on
        /// Absolute : follows the curve
        /// Additive : adds to the current scale of the target
        /// ToDestination : sets the scale to the destination target, whatever the current scale is
        public Modes Mode = Modes.Absolute;
        /// whether this feedback should play in scaled or unscaled time
        public TimeScales TimeScale = TimeScales.Scaled;
        /// the object to animate
        public SkinnedMeshRenderer BlendShapeAnimateTarget;

        /// the index of the blend shape to animate
        public int BlendShapeIndex;
        /// the duration of the animation
        public float AnimateScaleDuration = 0.2f;
        /// the value to remap the curve's 0 value to
        public float RemapCurveZero = 1f;
        /// the value to remap the curve's 1 value to
        [FormerlySerializedAs("Multiplier")]
        public float RemapCurveOne = 2f;
        /// how much should be added to the curve
        public float Offset = 0f;
        /// the blend shape
        public AnimationCurve AnimateBlendShape = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0));

        [Header("To Destination")] [MMFEnumCondition("Mode", (int) Modes.ToDestination)]
        public float DestinationValue = 1f;

        /// the duration of this feedback is the duration of the scale animation
        public override float FeedbackDuration { get { return AnimateScaleDuration; } }

        protected float _initialValue;
        protected float _newValue;

        /// <summary>
        /// On init we store our initial scale
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (Active && (BlendShapeAnimateTarget != null))
            {
                _initialValue = BlendShapeAnimateTarget.GetBlendShapeWeight(BlendShapeIndex);
            }
        }

        /// <summary>
        /// On Play, triggers the scale animation
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active && (BlendShapeAnimateTarget != null))
            {
                if (isActiveAndEnabled)
                {
                    if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
                    {
                        StartCoroutine(AnimateScale(BlendShapeAnimateTarget, 0f, AnimateScaleDuration,
                            AnimateBlendShape,  RemapCurveZero, RemapCurveOne));
                    }
                    if (Mode == Modes.ToDestination)
                    {
                        StartCoroutine(ScaleToDestination());
                    }                    
                }
            }
        }

        /// <summary>
        /// An internal coroutine used to scale the target to its destination scale
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ScaleToDestination()
        {
            if (BlendShapeAnimateTarget == null)
            {
                yield break;
            }

            if (AnimateBlendShape == null)
            {
                yield break;
            }

            if (AnimateScaleDuration == 0f)
            {
                yield break;
            }

            float journey = 0f;

            _initialValue = BlendShapeAnimateTarget.GetBlendShapeWeight(BlendShapeIndex);
            _newValue = _initialValue;

            while (journey < AnimateScaleDuration)
            {
                float percent = Mathf.Clamp01(journey / AnimateScaleDuration);
                
                    _newValue = Mathf.LerpUnclamped(_initialValue, DestinationValue, AnimateBlendShape.Evaluate(percent) + Offset);
                    _newValue = MMFeedbacksHelpers.Remap(_newValue, 0f, 1f, RemapCurveZero, RemapCurveOne);    
                
                    
                BlendShapeAnimateTarget.SetBlendShapeWeight(BlendShapeIndex, _newValue);

                journey += (TimeScale == TimeScales.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;
                yield return null;
            }

            BlendShapeAnimateTarget.SetBlendShapeWeight(BlendShapeIndex, DestinationValue);

            yield return null;
        }

        /// <summary>
        /// An internal coroutine used to animate the scale over time
        /// </summary>
        /// <param name="targetRenderer"></param>
        /// <param name="vector"></param>
        /// <param name="duration"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected virtual IEnumerator AnimateScale(SkinnedMeshRenderer targetRenderer, float value, float duration, 
            AnimationCurve curveX, float remapCurveZero = 0f, float remapCurveOne = 1f)
        {
            if (targetRenderer == null)
            {
                yield break;
            }

            if (curveX == null)
            {
                yield break;
            }

            if (duration == 0f)
            {
                yield break;
            }

            float journey = 0f;
            _initialValue = targetRenderer.GetBlendShapeWeight(BlendShapeIndex);
            
            while (journey < duration)
            {
                value = 0f;
                float percent = Mathf.Clamp01(journey / duration);


                value = curveX.Evaluate(percent) + Offset;
                value = MMFeedbacksHelpers.Remap(value, 0f, 1f, RemapCurveZero, RemapCurveOne);
                if (Mode == Modes.Additive)
                {
                    value += _initialValue;
                }

                
                targetRenderer.SetBlendShapeWeight(BlendShapeIndex, value);

                journey += (TimeScale == TimeScales.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;
                yield return null;
            }

            value = 0f;

            value = curveX.Evaluate(1f) + Offset;
            value = MMFeedbacksHelpers.Remap(value, 0f, 1f, RemapCurveZero, RemapCurveOne);
            if (Mode == Modes.Additive)
            {
                value += _initialValue;
            }

            targetRenderer.SetBlendShapeWeight(BlendShapeIndex, value);

            yield return null;
        }
    }
