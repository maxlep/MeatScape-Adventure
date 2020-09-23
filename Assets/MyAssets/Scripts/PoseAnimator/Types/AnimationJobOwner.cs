using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyAssets.Scripts.PoseAnimator.Types
{
    [Serializable]
    public abstract class AnimationJobOwner
    {
        public BoneTransformWeight[] BoneTransformWeights;
        private List<List<int>> boneChildrenIndices;
        
        public Playable Output => scriptPlayable;
        
        protected SharedAnimationData sharedData;
        protected AnimationScriptPlayable scriptPlayable;

        // Data used by the animation job
        protected NativeArray<float> boneWeights;
        protected NativeArray<TransformStreamHandle> boneHandles;
        
        #region Lifecycle methods
        public void Initialize(
            SharedAnimationData sharedData,
            BoneTransformWeight[] boneTransformWeights)
        {
            this.sharedData = sharedData;
            BoneTransformWeights = boneTransformWeights;

            boneHandles = sharedData.GetBoneHandlesCopy();
            boneWeights = sharedData.GetBoneWeightsCopy();

            if (boneTransformWeights.Length == 0 || boneTransformWeights[0].transform == null)
            {
                var defaultBoneWeight = new BoneTransformWeight
                {
                    // TODO remove this hardcoded default
                    transform = sharedData.AllAnimatorTransforms.First(i => i.name == "Root"),
                    weight = 1f,
                };

                if (boneTransformWeights.Length == 0) boneTransformWeights.Append(defaultBoneWeight);
                else boneTransformWeights[0] = defaultBoneWeight;
            }

            // Set bone weights for selected transforms and their hierarchy.
            boneChildrenIndices = new List<List<int>>(boneTransformWeights.Length);
            foreach (var boneTransform in boneTransformWeights)
            {
                var childrenTransforms = boneTransform.transform.GetComponentsInChildren<Transform>();
                var childrenIndices = new List<int>(childrenTransforms.Length);
                foreach (var childTransform in childrenTransforms)
                {
                    var boneIndex = Array.IndexOf(sharedData.AllAnimatorTransforms, childTransform);
                    Debug.Assert(boneIndex > 0, "Index can't be less or equal to 0");
                    childrenIndices.Add(boneIndex - 1);
                }
        
                boneChildrenIndices.Add(childrenIndices);
            }

            scriptPlayable = InitializePlayable();
        }

        public void OnDestroy()
        {
            boneHandles.Dispose();
            boneWeights.Dispose();
        }

        public void Update()
        {
            UpdateWeights();
        }
        #endregion

        protected abstract AnimationScriptPlayable InitializePlayable();

        private void UpdateWeights()
        {
            for (var i = 0; i < BoneTransformWeights.Length; ++i)
            {
                var boneWeight = BoneTransformWeights[i].weight;
                var childrenIndices = boneChildrenIndices[i];
                foreach (var index in childrenIndices)
                    boneWeights[index] = boneWeight;
            }
        }
    }
}