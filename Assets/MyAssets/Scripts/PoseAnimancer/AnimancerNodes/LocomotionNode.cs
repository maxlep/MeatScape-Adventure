using System;
using System.Linq;
using Animancer;
using Animancer.Examples.Jobs;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using Den.Tools.Matrices;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.PoseAnimancer.AnimancerJobs;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using Damping = MyAssets.Scripts.PoseAnimancer.AnimancerJobs.Damping;
using DampingJob = MyAssets.Scripts.PoseAnimancer.AnimancerJobs.DampingJob;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class LocomotionNode : AnimationStateNode
    {
        [TabGroup("Base","Animation"),SerializeField] private AvatarMask _locomotionMask;
        [TabGroup("Base","Animation"),SerializeField] private MixerState.Transition2D _move;
        [TabGroup("Base","Animation"),SerializeField] private float _leanFactor;
        
        [TabGroup("Base", "Inputs")]
        
        [TabGroup("Base/Inputs", "Locomotion")]
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private Vector3Reference _velocity;
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private FloatValueReference _maxSpeed;
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private Vector3Reference _groundNormal;
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private FloatValueReference _runBlendFactor;
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private FloatValueReference _moveInputFactor;
        [TitleGroup("Base/Inputs/Locomotion"),SerializeField] private FloatValueReference _bakedStrideLength;
        [TitleGroup("Base/Inputs/Locomotion"), SerializeField] private FloatValueReference _targetStrideLength;
        [TitleGroup("Base/Inputs/Locomotion"), SerializeField] private CurveReference _strideLeapFactor;
        [TitleGroup("Base/Inputs/Locomotion"), SerializeField] private CurveReference _strideLandFactor;
        
        [TabGroup("Base/Inputs", "Lean")]
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private Vector3Reference _acceleration;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private Vector3Reference _leanForwardAxis;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private FloatValueReference _leanForwardAngle;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private Vector3Reference _leanSideAxis;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private FloatValueReference _leanSideAngle;
        [TitleGroup("Base/Inputs/Lean"),SerializeField] private TransformSceneReference[] _leanBones;
        
        [TabGroup("Base/Inputs", "Bob")]
        [TitleGroup("Base/Inputs/Bob"),SerializeField] private Vector3Reference _bobAxis;
        [TitleGroup("Base/Inputs/Bob"),SerializeField] private FloatValueReference _bobConstantOffset;
        [TitleGroup("Base/Inputs/Bob"),SerializeField] private TransformSceneReference[] _bobBones;
        [TitleGroup("Base/Inputs/Bob"),SerializeField] private FloatValueReference _bobGravity;
        
        [TabGroup("Base/Inputs", "Events")]
        [TitleGroup("Base/Inputs/Events"),SerializeField] private GameEvent _stepLiftoffEventLeft;
        [TitleGroup("Base/Inputs/Events"),SerializeField] private GameEvent _stepLandEventLeft;
        [TitleGroup("Base/Inputs/Events"),SerializeField] private GameEvent _stepLiftoffEventRight;
        [TitleGroup("Base/Inputs/Events"),SerializeField] private GameEvent _stepLandEventRight;

        // [TitleGroup("Base/Inputs/Damping"), SerializeField] private TransformSceneReference _dampingEndBone;
        // [TitleGroup("Base/Inputs/Damping"), SerializeField] private IntReference _dampingBoneCount;
        
        [TabGroup("Base","Debug"),ShowInInspector] private float _walkCycleLength;
        [TabGroup("Base","Debug"),ShowInInspector] private bool _isMidStep;
        [TabGroup("Base","Debug"),ShowInInspector] private float _currentStepTargetStrideLength;
        [TabGroup("Base","Debug"),ShowInInspector] private float _distanceValue;
        [TabGroup("Base","Debug"),ShowInInspector] private float _walkCyclePercent;
        [TabGroup("Base","Debug"),ShowInInspector] private float _lastWalkCyclePercent;
        [TabGroup("Base","Debug"),ShowInInspector] private float _walkSpeedFactor;
        
        private AnimancerEvent.Sequence _moveEvents;
        private AnimancerEvent _nextMoveEvent;
        private int _nextMoveEventIndex;

        private SpecificLean _leanForward;
        private SpecificLean _leanSide;
        private TranslateRoot _bob;
        // private Damping _damping;
        
        public override void Enter()
        {
            base.Enter();
            
            // _walkCycleLength = 2 * _strideLength.Value;
            _distanceValue = 0;
            _walkCyclePercent = 0;
            _lastWalkCyclePercent = 0;
            _walkSpeedFactor = 0;
            
            _walkCycleLength = 2 * _targetStrideLength.Value;
            _currentStepTargetStrideLength = _targetStrideLength.Value;
            _targetStrideLength.Subscribe(() =>
            {
                _walkCycleLength = 2 * _targetStrideLength.Value;
            });
            
            _animatable.Animancer.States.GetOrCreate(_move);
            // _animatable.Animancer.Layers[BaseLayer].SetMask(_locomotionMask);
            _startNode.AnimancerLayer.Play(_move, 0.5f, FadeMode.FixedDuration);

            _moveEvents = _move.Events;
            if (_moveEvents.Count > 0)
            {
                _nextMoveEventIndex = 0;
                _nextMoveEvent = _moveEvents[_nextMoveEventIndex];
            }

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
            _bob.Distance = 0 + _bobConstantOffset.Value;
            _bob.Axis = _bobAxis.Value;

            // _damping = new Damping(_animatable.Animancer.Playable, _dampingBoneCount.Value, _dampingEndBone.Value);
        }
        
        public override void Exit()
        {
            base.Exit();
            
            // var distanceReset = LeanTween.value(_animatable.Animancer.gameObject, _bob.Distance, 0, 0.2f).setOnUpdate(
            //     (float value) =>
            //     {
            //         _bob.Distance = value;
            //     }).setOnComplete(() =>
            // {
            //     _bob.Destroy();
            // });
            
            // _animatable.Animancer.Layers[BaseLayer].SetMask();
            _leanForward?.Destroy();
            _leanSide?.Destroy();
            _bob?.Destroy();
            // _damping.Destroy();
        }
        
        public override void Execute()
        {
            base.Execute();
            
            if (_move.State.IsActive)
            {
                if (!_isMidStep)
                {
                    _currentStepTargetStrideLength = _targetStrideLength.Value;
                    _walkCycleLength = 2 * _currentStepTargetStrideLength;
                }

                _lastWalkCyclePercent = _walkCyclePercent;
                
                var speed = _velocity.Value.xz().magnitude;
                _distanceValue = (_distanceValue + speed * Time.deltaTime) % _walkCycleLength;
                _walkCyclePercent = _distanceValue / _walkCycleLength;
                _walkSpeedFactor = speed / _maxSpeed.Value;

                var dilationFactor = _bakedStrideLength.Value / _currentStepTargetStrideLength;

                _move.State.Parameter = new Vector2(0, _runBlendFactor.Value);
                _move.State.Speed = 1;
                _move.State.NormalizedTime = _walkCyclePercent;
                
                // TODO WARNING Events may be double triggered
                // Animancer doesn't trigger events that are passed by setting normalizedTime, only events where normalizedTime happens to collide exactly
                // This ensures passed events are always triggered, but doesn't protect against double triggering by Animancer
                if (_moveEvents.Count > 0)
                {
                    if (_nextMoveEvent.normalizedTime > _lastWalkCyclePercent
                        && _nextMoveEvent.normalizedTime <= _walkCyclePercent)
                    {
                        _nextMoveEvent.Invoke(_move.State);
                        _nextMoveEventIndex = (_nextMoveEventIndex + 1) % _moveEvents.Count;
                        _nextMoveEvent = _moveEvents[_nextMoveEventIndex];
                    }
                }
                
                var stridePercent = (_walkCyclePercent * 2f) % 1;
                var leapPercent = _strideLeapFactor.Value.Evaluate(stridePercent);
                var landPercent = _strideLandFactor.Value.Evaluate(stridePercent);

                if (leapPercent >= 1 && landPercent >= 0.9f)
                {
                    _isMidStep = false;
                }
                else
                {
                    _isMidStep = false; // true; This is mid-step detection is currently disabled by this comment.
                }
                
                var leapTime = (_strideLeapFactor.MaxTime - _strideLeapFactor.MinTime) * _currentStepTargetStrideLength / speed;
                var leapHeight = Mathf.Abs(0.5f * _bobGravity.Value * Mathf.Pow(leapTime / 2f, 2));
                var leapVerticalSpeed = Mathf.Abs(_bobGravity.Value * leapTime / 2f);

                var currentLeapTime = leapTime * leapPercent;
                var currentLeapHeight = 0 + (leapVerticalSpeed * currentLeapTime) +
                                        (0.5f * _bobGravity.Value * Mathf.Pow(currentLeapTime, 2));
                currentLeapHeight = Mathf.Min(currentLeapHeight, _currentStepTargetStrideLength)
                                    * Mathf.Pow(Mathf.Clamp01(_moveInputFactor.Value), 2);
                
                var landTime = (_strideLandFactor.MaxTime - _strideLandFactor.MinTime) * _currentStepTargetStrideLength / speed;
                var currentLandHeight = (1 - Mathf.Abs(landPercent * 2 - 1)) * _bobConstantOffset.Value;
                _bob.Distance = currentLeapHeight + currentLandHeight;
                
                var leanForwardPercent = Vector3.Dot(_acceleration.Value.normalized, _animatable.transform.forward);
                _leanForward.Angle = _walkSpeedFactor * leanForwardPercent * _leanForwardAngle.Value;
                
                var leanSidePercent = Vector3.Dot(_acceleration.Value.normalized, _animatable.transform.right);
                _leanSide.Angle = _walkSpeedFactor * leanSidePercent * _leanSideAngle.Value;
            }
            else
            {
                _move.State.Parameter = Vector2.zero;
                _move.State.Speed = 0;
            }
        }

        private float BobShapingFunction(float x)
        {
            var scaledX = (x - 0.5f) * 2;
            var factor = Mathf.Pow(Mathf.Min(Mathf.Cos(scaledX), 1 - Mathf.Abs(scaledX)), 0.5f);
            var value = 1 - Mathf.Cos(factor * Mathf.PI) / 2 - 0.5f;
            return value;
        }

        private float DilatedSineShapingFunction(float x, float dilationPower)
        {
            var dilation = DilaterShapingFunction(x, dilationPower);
            var factor = (1 - Mathf.Cos(dilation * Mathf.PI)) / 2;
            return factor;
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

            if (_animatable == null) return; 

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
        
        #region Events
        public void RaiseStepLiftoffEventLeft()
        {
            if (_stepLiftoffEventLeft != null) _stepLiftoffEventLeft.Raise();
        }
        
        public void RaiseStepLandEventLeft()
        {
            if (_stepLandEventLeft != null) _stepLandEventLeft.Raise();
        }

        public void RaiseStepLiftoffEventRight()
        {
            if (_stepLiftoffEventRight != null) _stepLiftoffEventRight.Raise();
        }
        
        public void RaiseStepLandEventRight()
        {
            if (_stepLandEventRight != null) _stepLandEventRight.Raise();
        }
        
        #endregion
    }
}