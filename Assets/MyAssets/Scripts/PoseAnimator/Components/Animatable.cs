using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using MyAssets.Scripts.PoseAnimator.Types;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Animancer;

#if UNITY_EDITOR
using AssetUsageDetectorNamespace;
#endif

namespace MyAssets.Scripts.PoseAnimator.Components
{
    [ExecuteInEditMode]
    public class Animatable : MonoBehaviour
    {
        [SerializeField] private AnimancerComponent _animancerComponent;
        public AnimancerComponent Animancer => _animancerComponent;
        
        [SerializeField] private Animator animator;

        public Transform RootTransform => rootTransform.Value;
        [SerializeField] private TransformSceneReference rootTransform;
    }
}