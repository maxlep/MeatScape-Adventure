using MyAssets.Scripts.PoseAnimator.Components;
using MyAssets.Scripts.PoseAnimator.Types;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class AnimationLayerStartNode : StartNode
    {
        [SerializeField] public AnimationLayer AnimationLayer;
        [SerializeField] private AnimatableSceneReference animatable;
        public SharedAnimationData SharedData => animatable.Value.SharedData;

        public override void RuntimeInitialize()
        {
            base.RuntimeInitialize();

            Debug.Log($"{animatable} {animatable.Value}");
            AnimationLayer = new AnimationLayer(base.name, SharedData);
            AnimationLayer.Initialize();
            animatable.Value.RegisterAnimationLayer(this);
            // animationStateMachine.AnimationLayerStartNodes.ForEach((i, index) =>
            //     i.InjectAnimationLayer(new AnimationLayer(name, sharedData)));
        }

        protected override void OnValidate()
        {
            name = $"Animation Start {executionOrderIndex}";
        }
    }
}