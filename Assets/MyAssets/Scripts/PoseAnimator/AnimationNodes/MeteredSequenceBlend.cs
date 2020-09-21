using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class MeteredSequenceBlend : SequenceBlend
    {
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private Vector3Reference moveVelocity;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Sirenix.OdinInspector.ReadOnly] private float meter = 0;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [Range(0.5f, 5.0f)] public float strideLength = 2f;

        public override void Execute()
        {
            UpdateMeter();
            base.Execute();
        }

        void UpdateMeter()
        {
            meter += Time.deltaTime * moveVelocity.Value.magnitude;

            if (meter >= strideLength * 2)
            {
                meter %= strideLength;
            }

            var pct = meter / strideLength / 2;
            factor.Value = pct;
        }
    }
}
