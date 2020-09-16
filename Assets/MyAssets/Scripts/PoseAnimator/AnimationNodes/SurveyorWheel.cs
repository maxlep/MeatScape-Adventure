﻿using System;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class SurveyorWheel : PlayerStateNode
    {
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private Vector2Vec3Reference moveVelocity;
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private FloatReference strideLength;
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private FloatReference distance;
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private FloatReference cyclePercent;

        [HideIf("$zoom"), LabelWidth(120), SerializeField] private bool resetOnStart = true;
        [HideIf("$zoom"), LabelWidth(120), SerializeField, ShowIf("resetOnStart")] private float roundToMultiple = 1;
        [HideIf("$zoom"), LabelWidth(120), SerializeField, Sirenix.OdinInspector.ReadOnly] private Transform probe;

        private float cycleLength;
        private float radius;

        protected override void OnValidate()
        {
            base.OnValidate();

            UpdateCachedValues();
        }

        public override void RuntimeInitialize()
        {
            base.RuntimeInitialize();
            
            probe = playerController.transform;
        }

        public override void Enter()
        {
            base.Enter();
            
            UpdateCachedValues();
            if (resetOnStart)
            {
                var roundedPct = Mathf.Floor(cyclePercent.Value / roundToMultiple) * roundToMultiple;
                cyclePercent.Value = Mathf.Clamp(roundedPct, 0, 1);
                distance.Value = cyclePercent.Value * cycleLength;
                Debug.Log(cyclePercent.Value +" "+ distance.Value);
            }
        }

        private void UpdateCachedValues()
        {
            cycleLength = 2 * strideLength.Value;
            radius = cycleLength / (2 * Mathf.PI);
        }

        public override void Execute()
        {
            base.Execute();

            UpdateDistance();
        }
        
        private void UpdateDistance()
        {
            var distanceLastFrame = Time.deltaTime * moveVelocity.Value.magnitude;
            distance.Value += distanceLastFrame;

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