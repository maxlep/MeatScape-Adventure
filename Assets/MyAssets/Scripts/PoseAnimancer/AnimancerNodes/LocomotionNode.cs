using System;
using System.Linq;
using Animancer;
using Animancer.Examples.Jobs;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.PoseAnimancer.AnimancerJobs;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class LocomotionNode : AnimationStateNode
    {
        [TabGroup("Base","Animation"),SerializeField] private AvatarMask _locomotionMask;
        [TabGroup("Base","Animation"),SerializeField] private MixerState.Transition2D _move;
        [TabGroup("Base","Animation"), SerializeField] private float _leanFactor;
        
        [TabGroup("Base", "Inputs")]
        
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private FloatValueReference _strideLength;
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private Vector3Reference _velocity;
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private FloatValueReference _maxSpeed;
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private Vector3Reference _groundNormal;
        
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private Vector3Reference _acceleration;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private Vector3Reference _leanForwardAxis;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private FloatValueReference _leanForwardAngle;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private Vector3Reference _leanSideAxis;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private FloatValueReference _leanSideAngle;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private TransformSceneReference[] _leanBones;
        
        [TitleGroup("Base/Inputs/Bob"),SerializeField] private Vector3Reference _bobAxis;
        [TitleGroup("Base/Inputs/Bob"),SerializeField] private FloatValueReference _bobAmplitude;
        [TitleGroup("Base/Inputs/Bob"), SerializeField] private FloatValueReference _bobCompressionPercent;
        [TitleGroup("Base/Inputs/Bob"),SerializeField] private TransformSceneReference[] _bobBones;
        
        [TabGroup("Base","Debug"),ShowInInspector] private float _walkCycleLength;
        [TabGroup("Base","Debug"),ShowInInspector] private float _distanceValue;
        [TabGroup("Base","Debug"),ShowInInspector] private float _walkCyclePercent;
        [TabGroup("Base","Debug"),ShowInInspector] private float _walkSpeedFactor;
        
        private SpecificLean _leanForward;
        private SpecificLean _leanSide;
        private TranslateRoot _bob;
        
        private const int BaseLayer = 0;
        private const int ActionLayer = 1;
        
        public override void Enter()
        {
            base.Enter();
            
            _walkCycleLength = 2 * _strideLength.Value;
            _distanceValue = 0;
            _walkCyclePercent = 0;
            _walkSpeedFactor = 0;
            
            _animatable.Animancer.Layers[ActionLayer].SetMask(_locomotionMask);
            _animatable.Animancer.States.GetOrCreate(_move);
            _animatable.Animancer.Play(_move, 0.5f, FadeMode.FixedDuration);

            // Init lean
            _leanForward = new SpecificLean(_animatable.Animancer.Playable, _leanBones.Select(b => b.Value));
            // _leanAngle.Subscribe(() =>
            // {
            //     _lean.Angle = _leanAngle.Value;
            // });
            _leanForwardAxis.Subscribe(() =>
            {
                _leanForward.Axis = _leanForwardAxis.Value;
            });
            _leanForward.Angle = _leanForwardAngle.Value;
            _leanForward.Axis = _leanForwardAxis.Value;
            
            _leanSide = new SpecificLean(_animatable.Animancer.Playable, _leanBones.Select(b => b.Value));
            // _leanSideAngle.Subscribe(() =>
            // {
            //     _leanSide.Angle = _leanSideAngle.Value;
            // });
            _leanSideAxis.Subscribe(() =>
            {
                _leanSide.Axis = _leanSideAxis.Value;
            });
            _leanSide.Angle = _leanSideAngle.Value;
            _leanSide.Axis = _leanSideAxis.Value;
            
            // Init bob
            _bob = new TranslateRoot(_animatable.Animancer.Playable, _bobBones.Select(b => b.Value));
            // _bobAmplitude.Subscribe(() =>
            // {
            //     _bob.Distance = _bobAmplitude.Value;
            // });
            _bobAxis.Subscribe(() =>
            {
                _bob.Axis = _bobAxis.Value;
            });
            _bob.Distance = _bobAmplitude.Value;
            _bob.Axis = _bobAxis.Value;
        }
        
        public override void Exit()
        {
            base.Exit();

            _leanForward.Destroy();
            _leanSide.Destroy();
            _bob.Destroy();
        }
        
        public override void Execute()
        {
            base.Execute();
            
            if (_move.State.IsActive)
            {
                var speed = _velocity.Value.xz().magnitude;
                _distanceValue = (_distanceValue + speed * Time.deltaTime) % _walkCycleLength;
                _walkCyclePercent = _distanceValue / _walkCycleLength;
                _walkSpeedFactor = speed / _maxSpeed.Value;
                
                _move.State.Parameter = new Vector2(0, _walkSpeedFactor);
                _move.State.Speed = 1;
                _move.State.NormalizedTime = _walkCyclePercent;

                var leanForwardPercent = Vector3.Dot(_acceleration.Value.normalized, _animatable.transform.forward);
                // _lean.Angle = Mathf.Min(leanPercent * _leanAngle.Value, _leanAngle.Value);
                _leanForward.Angle = _walkSpeedFactor * leanForwardPercent * _leanForwardAngle.Value;
                
                var leanSidePercent = Vector3.Dot(_acceleration.Value.normalized, _animatable.transform.right);
                // _lean.Angle = Mathf.Min(leanPercent * _leanAngle.Value, _leanAngle.Value);
                _leanSide.Angle = _walkSpeedFactor * leanSidePercent * _leanSideAngle.Value;
                
                var stridePercent = (_walkCyclePercent * 2f) % 1;
                var bobPercent = DilaterShapingFunction(stridePercent, 3);
                bobPercent = bobPercent * (1 + _bobCompressionPercent.Value) - _bobCompressionPercent.Value;
                _bob.Distance = Mathf.Clamp(bobPercent * _bobAmplitude.Value / _walkSpeedFactor, -_bobAmplitude.Value, _bobAmplitude.Value);
            }
            else
            {
                _move.State.Parameter = Vector2.zero;
                _move.State.Speed = 0;
            }
        }
        
        private float DilaterShapingFunction(float x, float dilationPower)
        {
            var remapX = (x * 2) - 1;
            var dilated = 1 - Mathf.Pow(Math.Abs(remapX), dilationPower);
            return dilated;
        }

        public override void DrawGizmos()
        {
            base.DrawGizmos();

            var t = _animatable.transform;
            var up = t.up;
            var forward = t.forward;
            var right = t.right;
            var groundContact = t.position;

            var radius = _walkCycleLength / (2 * Mathf.PI);
            var center = groundContact + (radius * up);

            var degrees = _walkCyclePercent * 180;
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