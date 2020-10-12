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
    public class AnimationJobRunner
    {
        [SerializeField] [ListDrawerSettings(Expanded = true)]
        [LabelWidth(165f)] public List<BoneTransformWeight> BoneTransformWeights;// = new List<BoneTransformWeight>();
        private List<List<int>> boneChildrenIndices;
        
        public string Name { get; private set; }
        public Playable Output => scriptPlayable;
        
        protected SharedAnimationData sharedData;
        protected AnimationScriptPlayable scriptPlayable;

        // Data used by the animation job
        protected NativeArray<float> boneWeights;
        protected NativeArray<TransformStreamHandle> boneHandles;
        
        #region Lifecycle methods
        public void Initialize(
            string name,
            SharedAnimationData sharedData)
        {
            Name = name;
            this.sharedData = sharedData;

            boneHandles = sharedData.GetBoneHandlesCopy();
            boneWeights = sharedData.GetBoneWeightsCopy();

            if (BoneTransformWeights == null || BoneTransformWeights.Count == 0 || BoneTransformWeights[0].transform == null)
            {
                var defaultBoneWeight = new BoneTransformWeight
                {
                    // TODO remove this hardcoded default
                    transform = sharedData.Animatable.RootTransform,
                    weight = 1f,
                };

                if (BoneTransformWeights == null) BoneTransformWeights = new List<BoneTransformWeight>();
                else if (BoneTransformWeights.Count == 0) BoneTransformWeights.Add(defaultBoneWeight);
                else BoneTransformWeights[0] = defaultBoneWeight;
            }

            // Set bone weights for selected transforms and their hierarchy.
            boneChildrenIndices = new List<List<int>>(BoneTransformWeights.Count);
            foreach (var boneTransform in BoneTransformWeights)
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

        public virtual void Dispose()
        {
            boneHandles.Dispose();
            boneWeights.Dispose();
        }

        public virtual void Update()
        {
            UpdateWeights();
            UpdateJob();
        }
        #endregion

        protected virtual AnimationScriptPlayable InitializePlayable()
        {
            throw new NotImplementedException();
        }

        protected virtual void UpdateJob()
        {
            throw new NotImplementedException();
        }

        private void UpdateWeights()
        {
            for (var i = 0; i < BoneTransformWeights.Count; ++i)
            {
                var boneWeight = BoneTransformWeights[i].weight;
                var childrenIndices = boneChildrenIndices[i];
                foreach (var index in childrenIndices)
                    boneWeights[index] = boneWeight;
            }
        }
    }
}