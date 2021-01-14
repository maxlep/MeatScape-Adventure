using System;
using MyAssets.Scripts.PoseAnimator.Components;
using MyAssets.Scripts.PoseAnimator.Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class AnimationLayerStartNode : StartNode
    {
        [LabelWidth(165f)] public bool isAdditive = false;
        
        [LabelWidth(165f)] [PropertyRange(0f, 1f)]
        public float layerWeight = 1f;

        [LabelWidth(165f)] public AvatarMask layerMask;
        
        [SerializeField] [LabelWidth(165f)]
        private AnimationClip defaultPose;

        [SerializeField] [LabelWidth(165f)] [PropertySpace(0f, 15f)]
        private AnimatableSceneReference animatable;

        [SerializeField] [LabelWidth(165f)] [PropertySpace(10f, 15f)] 
        [HideLabel] [BoxGroup("AnimationLayer")]
        public AnimationLayer AnimationLayer;

        public SharedAnimationData SharedData => animatable.Value.SharedData;
        

        public override void RuntimeInitialize()
        {
            base.RuntimeInitialize();

            // Debug.Log($"{animatable} {animatable.Value}");
            AnimationLayer = new AnimationLayer(base.name, SharedData, defaultPose, isAdditive, layerWeight, layerMask);
            AnimationLayer.Initialize();
            animatable.Value.RegisterAnimationLayer(this);
            // animationStateMachine.AnimationLayerStartNodes.ForEach((i, index) =>
            //     i.InjectAnimationLayer(new AnimationLayer(name, sharedData)));
        }

        protected override void OnValidate()
        {
            name = $"Animation Start {executionOrderIndex}";
        }

        public override void OnApplictionExit()
        {
            base.OnApplictionExit();
            AnimationLayer.Dispose();
        }
    }
}