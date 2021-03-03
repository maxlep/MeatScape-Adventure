using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerJobs
{
    public class JobUtils
    {
        public static NativeArray<TransformStreamHandle> GetDefaultHumanoidLeanBones(Animator animator)
        {
            var leanBones = new NativeArray<TransformStreamHandle>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            leanBones[0] = animator.BindStreamTransform(spine);
            var chest = animator.GetBoneTransform(HumanBodyBones.Head);
            leanBones[1] = animator.BindStreamTransform(chest);
            return leanBones;
        }

        public static NativeArray<TransformStreamHandle> GetTransformStreamHandles(Animator animator, IEnumerable<Transform> leanTransforms)
        {
            var transformStreamHandles = leanTransforms.Select(animator.BindStreamTransform).ToArray();
            var leanBones = new NativeArray<TransformStreamHandle>(transformStreamHandles, Allocator.Persistent);
            return leanBones;
        }
    }
}