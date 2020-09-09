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
            if (stream.inputStreamCount == 0)
            {
                return;
            }
        
            var streamA = stream.GetInputStream(0);
            var numHandles = handles.Length;

            if (stream.inputStreamCount < 2)
            {
                for (var i = 0; i < numHandles; ++i)
                {
                    var handle = handles[i];

                    var posA = handle.GetLocalPosition(streamA);
                    handle.SetLocalPosition(stream, posA);

                    var rotA = handle.GetLocalRotation(streamA);
                    handle.SetLocalRotation(stream, rotA);
                }

                return;
            }

            var streamB = stream.GetInputStream(1);

            for (var i = 0; i < numHandles; ++i)
            {
                var handle = handles[i];

                var posA = handle.GetLocalPosition(streamA);
                var posB = handle.GetLocalPosition(streamB);
                handle.SetLocalPosition(stream, Vector3.Lerp(posA, posB, weight * boneWeights[i]));

                var rotA = handle.GetLocalRotation(streamA);
                var rotB = handle.GetLocalRotation(streamB);
                handle.SetLocalRotation(stream, Quaternion.Slerp(rotA, rotB, weight * boneWeights[i]));
            }
        }
    }
}
