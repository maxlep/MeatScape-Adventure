using MyAssets.Scripts.PoseAnimator.Components;
using MyAssets.Scripts.PoseAnimator.Types;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class AnimationLayerStartNode : StartNode
    {
        public Animatable Animatable;
        public AnimationLayer AnimationLayer { get; private set; }
        public SharedAnimationData SharedData => Animatable.SharedData;

        public override void RuntimeInitialize()
        {
            base.RuntimeInitialize();
        }

        public void InjectAnimationLayer(AnimationLayer animationLayer)
        {
            AnimationLayer = animationLayer;
        }

        protected override void OnValidate()
        {
            name = $"Animation Start {executionOrderIndex}";
        }
    }
}