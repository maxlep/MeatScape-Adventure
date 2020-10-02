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
        public NativeArray<TransformStreamHandle> handles;
        public NativeArray<BoneLocation> bonesLastFrame;
        public NativeArray<BoneLocation> bonesLastState;
        public bool useBonesLastAsFirst, isTransitioning;
        public NativeArray<float> boneWeights;
        public float weight;

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

            if (useBonesLastAsFirst)
            {
                if (!isTransitioning)
                {
                    isTransitioning = true;
                    bonesLastFrame.CopyTo(bonesLastState);
                }

                for (var i = 0; i < numHandles; ++i)
                {
                    var handle = handles[i];

                    var posA = bonesLastFrame[i].position;
                    var posB = handle.GetLocalPosition(streamB);
                    var pos = Vector3.Lerp(posA, posB, weight * boneWeights[i]);
                    handle.SetLocalPosition(stream, pos);

                    var rotA = bonesLastFrame[i].rotation;
                    var rotB = handle.GetLocalRotation(streamB);
                    var rot = Quaternion.Slerp(rotA, rotB, weight * boneWeights[i]);
                    handle.SetLocalRotation(stream, rot);
                    
                    bonesLastFrame[i] = new BoneLocation(pos, rot);
                }

                return;
            }
            
            if (stream.inputStreamCount < 2)
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

                var posA = handle.GetLocalPosition(streamA);
                var posB = handle.GetLocalPosition(streamB);
                var pos = Vector3.Lerp(posA, posB, weight * boneWeights[i]);
                handle.SetLocalPosition(stream, pos);

                var rotA = handle.GetLocalRotation(streamA);
                var rotB = handle.GetLocalRotation(streamB);
                var rot = Quaternion.Slerp(rotA, rotB, weight * boneWeights[i]);
                handle.SetLocalRotation(stream, rot);
                
                bonesLastFrame[i] = new BoneLocation(pos, rot);
            }
        }
    }
}
