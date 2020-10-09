using System;
using Sirenix.OdinInspector;
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
    public class SequenceUnit
    {
        public AnimationClip pose;
        public AnimationCurve transition;
        
        [PropertyRange(0f ,1f)] [OnValueChanged("RoundInput")] [DisableIf("$useMonospacedSequence")]
        public float timeLinePosition;

        private const float roundInterval = 100f;
        private bool useMonospacedSequence;

        public void EnableUseMonospacedSequence(float monoTimeLinePos)
        {
            useMonospacedSequence = true;
            timeLinePosition = monoTimeLinePos;
        }
        
        public void DisableUseMonospacedSequence()
        {
            useMonospacedSequence = false;
        }
        
        private void RoundInput()
        {
            timeLinePosition = Mathf.Round(timeLinePosition * roundInterval) / roundInterval;
        }
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

    [Serializable]
    public enum BlendMode
    {
        Mix,
        Add,
        Multiply,
    }
}