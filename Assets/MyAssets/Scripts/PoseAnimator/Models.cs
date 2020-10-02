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
    
    [Serializable]
    public struct BoneLocation
    {
        public Vector3 position;
        public Quaternion rotation;

        public BoneLocation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}