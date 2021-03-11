using System;
using System.Collections.Generic;
using MyAssets.Scripts.PoseAnimator;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
#if UNITY_2019_3_OR_NEWER

#else
using UnityEngine.Experimental.Animations;
#endif

namespace MyAssets.Runtime.AnimationJobs
{
    public struct MixerJob : IAnimationJob
    {
        public BlendMode blendMode;
        public NativeArray<TransformStreamHandle> handles;
        public NativeArray<BoneLocation> bonesLastFrame;
        public NativeArray<BoneLocation> bonesLastState;
        public bool useBonesLastAsFirst, isTransitioning;
        public NativeArray<float> boneWeights;
        public float weight;
        
        private static Dictionary<BlendMode, Func<Vector3, Vector3, float, Vector3>> Vector3Blend = new Dictionary<BlendMode, Func<Vector3, Vector3, float, Vector3>>
        {
            {BlendMode.Mix, (vec1, vec2, fac) => Vector3.Lerp(vec1, vec2, fac)},
            {BlendMode.Add, (vec1, vec2, fac) => vec1 + (vec2 * fac)},
        };
        private static Dictionary<BlendMode, Func<Quaternion, Quaternion, float, Quaternion>> QuaternionBlend = new Dictionary<BlendMode, Func<Quaternion, Quaternion, float, Quaternion>>
        {
            {BlendMode.Mix, (rot1, rot2, fac) => Quaternion.Slerp(rot1, rot2, fac)},
            {BlendMode.Add, (rot1, rot2, fac) => rot1 * rot2},
        };

        public void ProcessRootMotion(AnimationStream stream)
        {
            // var streamA = stream.GetInputStream(0);
            // var streamB = stream.GetInputStream(1);
            //
            // var velocity = Vector3.Lerp(streamA.velocity, streamB.velocity, weight);
            // var angularVelocity = Vector3.Lerp(streamA.angularVelocity, streamB.angularVelocity, weight);
            // stream.velocity = velocity;
            // stream.angularVelocity = angularVelocity;
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            // Debug.Log($"Process animation {stream.isValid}");
            if (stream.inputStreamCount == 0)
            {
                return;
            }
            
            var streamA = stream.GetInputStream(0);
            var streamB = stream.GetInputStream(1);
            var numHandles = handles.Length;

            if (useBonesLastAsFirst &&
                streamA.isValid)
            {
                if (!isTransitioning)
                {
                    isTransitioning = true;
                    bonesLastFrame.CopyTo(bonesLastState);
                    // Debug.LogError("Start new mixer transition");
                }

                for (var i = 0; i < numHandles; ++i)
                {
                    var handle = handles[i];

                    var posA = bonesLastState[i].position;
                    var posB = handle.GetLocalPosition(streamA);
                    var pos = Vector3Blend[blendMode](posA, posB, weight * boneWeights[i]);
                    handle.SetLocalPosition(stream, pos);

                    var rotA = bonesLastState[i].rotation;
                    var rotB = handle.GetLocalRotation(streamA);
                    var rot = QuaternionBlend[blendMode](rotA, rotB, weight * boneWeights[i]);
                    handle.SetLocalRotation(stream, rot);
                    
                    bonesLastFrame[i] = new BoneLocation(pos, rot);
                }

                return;
            }

            if (!streamA.isValid && !streamB.isValid) return;
            
            if (stream.inputStreamCount < 2 || !streamB.isValid)
            {
                for (var i = 0; i < numHandles; ++i)
                {
                    var handle = handles[i];

                    var posA = handle.GetLocalPosition(streamA);
                    handle.SetLocalPosition(stream, posA);

                    var rotA = handle.GetLocalRotation(streamA);
                    handle.SetLocalRotation(stream, rotA);
                    
                    bonesLastFrame[i] = new BoneLocation(posA, rotA);
                }

                return;
            }

            for (var i = 0; i < numHandles; ++i)
            {
                var handle = handles[i];
            
                var posA = handle.GetLocalPosition(streamB);
                var posB = handle.GetLocalPosition(streamA);
                var pos = Vector3Blend[blendMode](posA, posB, weight * boneWeights[i]);
                handle.SetLocalPosition(stream, pos);
            
                var rotA = handle.GetLocalRotation(streamB);
                var rotB = handle.GetLocalRotation(streamA);
                var rot = QuaternionBlend[blendMode](rotA, rotB, weight * boneWeights[i]);
                handle.SetLocalRotation(stream, rot);
                
                bonesLastFrame[i] = new BoneLocation(pos, rot);
            }
        }
    }
}
