using System;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Transform/Scale Extended")]
    [FeedbackHelp("This feedback will animate the target's scale on the 3 specified animation curves, for the specified duration (in seconds). You can apply a multiplier, that will multiply each animation curve value.")]
    public class MMFeedbackScaleExtended : MMFeedbackScale
    {
        // [Header("To Destination")]
        [MMFEnumCondition("Mode", (int) Modes.ToDestination)]
        public Vector3Variable DestinationScaleReference;

        private void UpdateDestinationScale()
        {
            DestinationScale = DestinationScaleReference.GetVector3();
        }

        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1)
        {
            if (DestinationScaleReference != null)
            {
                UpdateDestinationScale();
            }
            
            base.CustomPlayFeedback(position, attenuation);
        }
    }
}