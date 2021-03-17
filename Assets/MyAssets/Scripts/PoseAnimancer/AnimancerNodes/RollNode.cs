using Animancer;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class RollNode : AnimationStateNode
    {
        [TabGroup("Base","Animation"), SerializeField] private ClipState.Transition _pose;
        
        [TabGroup("Base/Inputs", "Locomotion")]
        [TitleGroup("Base/Inputs/Locomotion"), SerializeField] private FloatValueReference _radius;
        [TitleGroup("Base/Inputs/Locomotion"), SerializeField] private Vector3Reference _velocity;
        [TitleGroup("Base/Inputs/Locomotion"), SerializeField] private BoolReference _isGrounded;
        [TitleGroup("Base/Inputs/Locomotion"), SerializeField] private TransformSceneReference _pivot;
        
        private Quaternion startRotation;
        private float lastAngularDistance;
        
        public override void Enter()
        {
            base.Enter();
            
            startRotation = _pivot.Value.transform.localRotation;
            _animatable.Animancer.States.GetOrCreate(_pose);
            _animatable.Animancer.Layers[0].Play(_pose);
        }
        
        public override void Exit()
        {
            base.Exit();
            
            _pivot.Value.transform.localRotation = startRotation;
        }
        
        public override void Execute()
        {
            base.Execute();
            
            UpdateRotation();
        }   
        
        private void UpdateRotation()
        {
            var axis = Vector3.Cross(_velocity.Value.xoz(), Vector3.up);
            // Should really use the ground normal in place of Vector3.up, but this assumption may be fine for flat-ish terrain

            if (_isGrounded.Value)
            {
                var distance = _velocity.Value.magnitude * Time.smoothDeltaTime;
                // Remember to change delta time if this is moved to fixed update

                var circumference = Mathf.PI * _radius.Value * 2;
                lastAngularDistance = distance / circumference * 360 * -1;
            }
            var rotationChange = Quaternion.AngleAxis(lastAngularDistance, axis);
            var newRotation = rotationChange * _pivot.Value.rotation;
            _pivot.Value.rotation = newRotation;

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
            Vector3 startPos = _pivot.Value.transform.position;

            var lines = new (Vector3 endPos, Color color)[]
            {
                (startPos + _velocity.Value * len,
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