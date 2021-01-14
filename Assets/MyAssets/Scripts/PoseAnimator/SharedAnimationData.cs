using System;
using System.Collections.Generic;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using MyAssets.Scripts.PoseAnimator.Components;
using MyAssets.Scripts.PoseAnimator.Types;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyAssets.Scripts.PoseAnimator
{
    [Serializable]
    public class SharedAnimationData
    {
        public Animatable Animatable { get; set; }
        public Animator Animator { get; private set; }
        public Transform[] AllAnimatorTransforms { get; private set; }
        
        public PlayableGraph PlayableGraph { get; private set; }

        // public Dictionary<int, AnimationLayer> AnimationLayers { get; set; }

        [NonSerialized] public List<AnimationLayer> AnimationLayers;
        public Dictionary<int, int> StartIndexToLayerIndex;

        public SharedAnimationData(
            Animator animator,
            PlayableGraph playableGraph)
        {
            Animator = animator;
            PlayableGraph = playableGraph;

            // Get all the transforms in the hierarchy.
            AllAnimatorTransforms = animator.transform.GetComponentsInChildren<Transform>();
            var numTransforms = AllAnimatorTransforms.Length - 1;
            
            // Fill native arrays (all the bones have a weight of 0.0).
            boneHandles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            boneWeights = new NativeArray<float>(numTransforms, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            for (var i = 0; i < numTransforms; ++i)
            {
                boneHandles[i] = animator.BindStreamTransform(AllAnimatorTransforms[i + 1]);
                boneWeights[i] = 1;
            }
            bonesLastFrame = new NativeArray<BoneLocation>(numTransforms, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        private NativeArray<TransformStreamHandle> boneHandles;
        private NativeArray<float> boneWeights;
        private NativeArray<BoneLocation> bonesLastFrame;

        public NativeArray<TransformStreamHandle> GetBoneHandlesCopy()
        {
            // return boneHandles;
            var copy = new NativeArray<TransformStreamHandle>(boneHandles.Length, Allocator.Persistent,
                NativeArrayOptions.ClearMemory);
            boneHandles.CopyTo(copy);
            return copy;
        }

        public NativeArray<float> GetBoneWeightsCopy()
        {
            // return boneWeights;
            var copy = new NativeArray<float>(boneWeights.Length, Allocator.Persistent,
                NativeArrayOptions.ClearMemory);
            boneWeights.CopyTo(copy);
            return copy;
        }
        
        public NativeArray<BoneLocation> GetBonesLastFrameCopy()
        {
            var copy = new NativeArray<BoneLocation>(bonesLastFrame.Length, Allocator.Persistent,
                NativeArrayOptions.ClearMemory);
            bonesLastFrame.CopyTo(copy);
            return copy;
        }

        public AnimationLayer GetAnimationLayer(int startNodeIndex)
        {
            var layerIndex = StartIndexToLayerIndex[startNodeIndex];
            return AnimationLayers[layerIndex];
        }

        public void Dispose()
        {
            boneHandles.Dispose();
            boneWeights.Dispose();
            bonesLastFrame.Dispose();
        }
    }
}