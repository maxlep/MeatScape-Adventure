using System.Linq;
using Animancer.Examples.Jobs;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.PoseAnimancer.AnimancerJobs;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class SpineLeanNode : AnimationStateNode
    {
        [TitleGroup("Parameters")]
        [SerializeField] private TransformSceneReference[] leanBones;
        [SerializeField] private FloatValueReference leanForwardMaxAngle;
        [SerializeField] private FloatValueReference leanSideMaxAngle;
        [SerializeField] private Vector3Reference leanForwardAxis;
        [SerializeField] private Vector3Reference leanSideAxis;

        [TitleGroup("Inputs")]
        [SerializeField] private Vector3Reference velocity;
        [SerializeField] private Vector3Reference acceleration;
        [SerializeField] private FloatValueReference maxSpeed;

        private SpecificLean _leanForward;
        private SpecificLean _leanSide;

        #region Lifecycle

        public override void Enter()
        {
            base.Enter();

            _leanForward = new SpecificLean(_animatable.Animancer.Playable, leanBones.Select(b => b.Value));
            _leanForward.Axis = leanForwardAxis.Value;
            leanForwardAxis.Subscribe(UpdateLeanForwardAxis);

            _leanSide = new SpecificLean(_animatable.Animancer.Playable, leanBones.Select(b => b.Value));
            _leanSide.Axis = leanSideAxis.Value;
            leanSideAxis.Subscribe(UpdateLeanSideAxis);

            acceleration.Subscribe(UpdateLeanAngles);
        }

        public override void Exit()
        {
            base.Exit();

            leanForwardAxis.Unsubscribe(UpdateLeanForwardAxis);
            leanSideAxis.Unsubscribe(UpdateLeanSideAxis);
            acceleration.Unsubscribe(UpdateLeanAngles);

            _leanForward.Destroy();
            _leanSide.Destroy();
        }

        public override void OnDestroy()
        {
            leanForwardAxis.Unsubscribe(UpdateLeanForwardAxis);
            leanSideAxis.Unsubscribe(UpdateLeanSideAxis);
            acceleration.Unsubscribe(UpdateLeanAngles);

            _leanForward.Destroy();
            _leanSide.Destroy();
        }

        #endregion

        #region Update methods

        private void UpdateLeanForwardAxis()
        {
            _leanForward.Axis = leanForwardAxis.Value;
        }

        private void UpdateLeanSideAxis()
        {
            _leanSide.Axis = leanSideAxis.Value;
        }

        private void UpdateLeanAngles()
        {
            var speed = velocity.Value.xz().magnitude;
            var walkSpeedFactor = speed / maxSpeed.Value;
            var leanForwardPercent = Vector3.Dot(acceleration.Value.normalized, _animatable.transform.forward);
            _leanForward.Angle = walkSpeedFactor * leanForwardPercent * leanForwardMaxAngle.Value;

            var leanSidePercent = Vector3.Dot(acceleration.Value.normalized, _animatable.transform.right);
            _leanSide.Angle = walkSpeedFactor * leanSidePercent * leanSideMaxAngle.Value;
        }

        #endregion
    }
}