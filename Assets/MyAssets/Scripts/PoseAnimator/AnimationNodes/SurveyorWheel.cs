using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class SurveyorWheel : PlayerStateNode
    {
        [SerializeField] private Vector3Reference moveVelocity;
        [SerializeField] private FloatReference strideLength;
        [SerializeField] private FloatReference distance;
        [SerializeField] private FloatReference cyclePercent;
        [SerializeField, Sirenix.OdinInspector.ReadOnly] private Transform probe;

        private float cycleLength;
        private float radius;

        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateCachedValues();
        }

        public override void Initialize(StateMachineGraph parentGraph)
        {
            base.Initialize(parentGraph);
        }

        public override void Enter()
        {
            UpdateCachedValues();
            probe = playerController.transform;
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
            var distanceLastFrame = Time.deltaTime * moveVelocity.Value.xz().magnitude;
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