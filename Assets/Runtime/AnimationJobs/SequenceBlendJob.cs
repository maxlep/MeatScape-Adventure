using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

#if UNITY_2019_3_OR_NEWER
using UnityEngine.Animations;
#else
using UnityEngine.Experimental.Animations;
#endif

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
        var streamA = stream.GetInputStream(poseAIndex);
        var streamB = stream.GetInputStream(poseBIndex);

        var numHandles = handles.Length;
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