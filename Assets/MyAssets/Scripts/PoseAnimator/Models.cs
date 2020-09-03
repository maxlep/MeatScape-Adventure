using System;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator
{
    [Serializable]
    public struct BoneTransformWeight
    {
        public Transform transform;

        [Range(0.0f, 1.0f)]
        public float weight;
    }

    [Serializable]
    public struct SequenceUnit
    {
        public AnimationClip pose;
        public AnimationCurve transition;
    }

    public enum ExtrapolateBehavior
    {
        Hold, Wrap, PingPong, Overshoot
    }
}