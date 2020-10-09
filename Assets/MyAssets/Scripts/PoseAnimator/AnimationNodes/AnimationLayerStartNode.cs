using MyAssets.Scripts.PoseAnimator.Components;
using MyAssets.Scripts.PoseAnimator.Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class AnimationLayerStartNode : StartNode
    {
        [SerializeField] [LabelWidth(165f)] [PropertySpace(0f, 15f)]
        private AnimatableSceneReference animatable;

        [SerializeField] [LabelWidth(165f)] [PropertySpace(10f, 15f)] 
        [HideLabel] [BoxGroup("AnimationLayer")]
        public AnimationLayer AnimationLayer;

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