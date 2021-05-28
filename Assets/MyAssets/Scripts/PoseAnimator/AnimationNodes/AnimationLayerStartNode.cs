using System;
using Animancer;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.PoseAnimator.Components;
using MyAssets.Scripts.PoseAnimator.Types;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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

        [SerializeField] private int animancerLayerIndex;

        public Animatable Animatable => animatable.Value;

        public AnimancerLayer AnimancerLayer => Animatable.Animancer.Layers[animancerLayerIndex];
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            name = $"Animation {name}";
        }

        public override void RuntimeInitialize()
        {
            base.RuntimeInitialize();

            if (!layerMask.SafeIsUnityNull())
            {
                AnimancerLayer.SetMask(layerMask);
            }
        }
    }
}