using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
#if UNITY_2019_3_OR_NEWER

#else
using UnityEngine.Experimental.Animations;
#endif

namespace MyAssets.Runtime.AnimationJobs
{
    public struct SequenceBlendJob : IAnimationJob
    {
        public NativeArray<TransformStreamHandle> handles;
        public NativeArray<float> boneWeights;
        public float weight;
        public int poseAIndex, poseBIndex;

        public void ProcessRootMotion(AnimationStream stream)
        {
            // var streamA = stream.GetInputStream(poseAIndex);
            // var streamB = stream.GetInputStream(poseBIndex);
            //
            // var velocity = Vector3.Lerp(streamA.velocity, streamB.velocity, weight);
            // var angularVelocity = Vector3.Lerp(streamA.angularVelocity, streamB.angularVelocity, weight);
            // stream.velocity = velocity;
            // stream.angularVelocity = angularVelocity;
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            if (stream.inputStreamCount == 0)
            {
                return;
            }

            if (stream.inputStreamCount == 1)
            {
                var streamA = stream.GetInputStream(poseAIndex);
                
                var numHandles = handles.Length;
                for (var i = 0; i < numHandles; ++i)
                {
                    var handle = handles[i];

                    var posA = handle.GetLocalPosition(streamA);
                    handle.SetLocalPosition(stream, posA);

                    var rotA = handle.GetLocalRotation(streamA);
                    handle.SetLocalRotation(stream, rotA);
                }
            }
            else
            {
                var streamA = stream.GetInputStream(poseAIndex);
                var streamB = stream.GetInputStream(poseBIndex);
                
                // Debug.Log($"Process animation {stream.inputStreamCount} {weight} {streamA.isValid} {streamB.isValid}");

                var numHandles = handles.Length;
                for (var i = 0; i < numHandles; ++i)
                {
                    var handle = handles[i];

                    var posA = handle.GetLocalPosition(streamA);
                    var posB = handle.GetLocalPosition(streamB);
                    handle.SetLocalPosition(stream, Vector3.Lerp(posA, posB, weight * boneWeights[i]));
                    // if (posA.x > 0 || posA.y > 0 || posA.z > 0) Debug.Log($"{posA.ToString("F4")} {posB.ToString("F4")} {handle.GetLocalPosition(stream).ToString("F4")} {boneWeights[i]} {weight} {weight * boneWeights[i]}");

                    var rotA = handle.GetLocalRotation(streamA);
                    var rotB = handle.GetLocalRotation(streamB);
                    handle.SetLocalRotation(stream, Quaternion.Slerp(rotA, rotB, weight * boneWeights[i]));
                }
            }
        }
    }
}
