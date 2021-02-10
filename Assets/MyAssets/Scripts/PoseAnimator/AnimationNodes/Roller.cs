using System;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using Unity.Transforms;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class Roller : StateNode
    {
        [TabGroup("Input"), HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField, Required]
        private Vector3Reference MoveVelocity;
        [TabGroup("Input"), HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField, Required]
        private FloatReference Radius;
        [TabGroup("Input"), HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField, Required]
        private BoolReference IsGrounded;
        
        [TabGroup("Output"), HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField, Required]
        private TransformSceneReference Pivot;

        private Quaternion startRotation;
        private float lastAngularDistance;

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

            UpdateRotation();
        }   

        private void UpdateRotation()
        {
            var axis = Vector3.Cross(MoveVelocity.Value.xoz(), Vector3.up);
            // Should really use the ground normal in place of Vector3.up, but this assumption may be fine for flat-ish terrain

            if (IsGrounded.Value)
            {
                var distance = MoveVelocity.Value.magnitude * Time.smoothDeltaTime;
                // Remember to change delta time if this is moved to fixed update

                var circumference = Mathf.PI * Radius.Value * 2;
                lastAngularDistance = distance / circumference * 360 * -1;
            }
            var rotationChange = Quaternion.AngleAxis(lastAngularDistance, axis);
            var newRotation = rotationChange * Pivot.Value.rotation;
            Pivot.Value.rotation = newRotation;

            rotationAxis = axis;
        }

        private Vector3 rotationAxis = new Vector3();

        public override void DrawGizmos()
        {
            // set up all static parameters. these are used for all following Draw.Line calls
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.LineThicknessSpace = ThicknessSpace.Pixels;
            Draw.LineThickness = 6; // 4px wide

            float len = 10f;
            float radius = 0.25f;
            float alpha = 0.35f;
            Vector3 startPos = Pivot.Value.transform.position;

            var lines = new (Vector3 endPos, Color color)[]
            {
                (startPos + MoveVelocity.Value * len,
                    Color.green),
                (startPos + rotationAxis * len,
                    Color.magenta),
            };

            foreach (var line in lines)
            {
                Draw.Line(startPos, line.endPos, line.color);
                Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, line.endPos, radius, line.color);
            }
        }
    }
}