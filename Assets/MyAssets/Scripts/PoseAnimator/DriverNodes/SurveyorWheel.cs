using System;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class SurveyorWheel : PlayerStateNode
    {
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private Vector2Vec3Reference moveVelocity;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatReference maxSpeed;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatReference walkSpeedFactor;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private float minStrideLengthFactor = 0.25f;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatReference fullStrideLength;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatReference strideLength;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatReference distance;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatReference cyclePercent;

        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private bool resetOnStart = true;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField, ShowIf("resetOnStart")] private float roundToMultiple = 1;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField, Sirenix.OdinInspector.ReadOnly] private Transform probe;

        private float cycleLength;
        private float radius;

        protected override void OnValidate()
        {
            base.OnValidate();

            // UpdateCachedValues();
        }

        public override void RuntimeInitialize(int startNodeIndex)
        {
            base.RuntimeInitialize(startNodeIndex);
            
            probe = playerController.transform;
        }

        public override void Enter()
        {
            base.Enter();
            
            // UpdateCachedValues();
            if (resetOnStart)
            {
                var roundedPct = Mathf.Floor(cyclePercent.Value / roundToMultiple) * roundToMultiple;
                cyclePercent.Value = Mathf.Clamp(roundedPct, 0, 1);
                distance.Value = cyclePercent.Value * cycleLength;
                // Debug.Log(cyclePercent.Value +" "+ distance.Value);
            }
        }

        // private void UpdateCachedValues()
        // {
        //     cycleLength = 2 * strideLength.Value;
        //     radius = cycleLength / (2 * Mathf.PI);
        // }

        public override void Execute()
        {
            base.Execute();

            UpdateDistance();
        }
        
        private void UpdateDistance()
        {
            var distanceLastFrame = Time.deltaTime * moveVelocity.Value.magnitude;
            distance.Value += distanceLastFrame;

            walkSpeedFactor.Value = moveVelocity.Value.sqrMagnitude / Mathf.Pow(maxSpeed.Value, 2);
            walkSpeedFactor.Value = Math.Max(walkSpeedFactor.Value, minStrideLengthFactor);
            strideLength.Value = walkSpeedFactor.Value * fullStrideLength.Value;
            cycleLength = 2 * fullStrideLength.Value;
            radius = cycleLength / (2 * Mathf.PI);
            
            // Debug.Log($"{walkSpeedFactor.Value}, {strideLength.Value}, {cycleLength}, {radius}");

            if (distance.Value >= cycleLength)
            {
                distance.Value %= cycleLength;
            }

            var pct = distance.Value / cycleLength;
            cyclePercent.Value = pct;
        }

        public override void DrawGizmos()
        {
            base.DrawGizmos();

            if (probe == null) return;

            var up = probe.up;
            var forward = probe.forward;
            var right = probe.right;
            var groundContact = probe.position;

            var center = groundContact + (radius * up);

            var degrees = distance.Value / strideLength.Value * 180;
            var rotation = Quaternion.AngleAxis(degrees, right);

            var extents = (
                contact: groundContact,
                center + (radius * up),
                center - radius * forward,
                center + radius * forward
            );
            var rotated = (
                extents.Item1.RotatePointAroundPivot(center, rotation), 
                extents.Item2.RotatePointAroundPivot(center, rotation), 
                extents.Item3.RotatePointAroundPivot(center, rotation), 
                extents.Item4.RotatePointAroundPivot(center, rotation)
            );

            Gizmos.DrawLine(rotated.Item1, rotated.Item2);
            GreatGizmos.DrawLine(rotated.Item3, rotated.Item4, LineStyle.Dashed);
        }
    }
}