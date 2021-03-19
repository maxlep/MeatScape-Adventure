using System;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
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

        public Animatable Animatable => animatable.Value;
        
        protected override void OnValidate()
        {
            name = $"Animation Start {executionOrderIndex}";
        }
    }
}