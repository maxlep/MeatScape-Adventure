using System;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class RotateAroundPivot : StateNode
    {
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        private TransformSceneReference Pivot;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        private List<RotationAxis> Rotations;

        [Serializable]
        private class RotationAxis
        {
            public Vector3Axes LocalAxis;
            public FloatReference Factor;
        }

        private Quaternion startRotation;

        public override void Enter()
        {
            base.Enter();

            startRotation = Pivot.Value.transform.localRotation;
        }
        
        public override void Exit()
        {
            base.Exit();

            Pivot.Value.transform.localRotation = startRotation;
        }

        public override void Execute()
        {
            base.Execute();

            var newRotation = Quaternion.identity;
            foreach (var rotationAxis in Rotations)
            {
                var axis = rotationAxis.LocalAxis.GetAxis();
                var deg = 360 * rotationAxis.Factor.Value;
                var rot = Quaternion.AngleAxis(deg, axis);
                newRotation *= rot;
            }

            Pivot.Value.transform.localRotation = newRotation;
        }
    }
}