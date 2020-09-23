using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MyAssets.Runtime.AnimationJobs;
using MyAssets.Scripts.PoseAnimator.Types;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public abstract class AnimationStateNode : StateNode
    {
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        private BoneTransformWeight[] boneTransformWeights;
        private List<List<int>> boneChildrenIndices;

        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        private AnimationJobOwner animationJobOwner;

        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        private float transitionTime;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        private AnimationCurve transitionCurve;

        public Playable Output => scriptPlayable;
        
        protected SharedAnimationData sharedData;
        protected AnimationScriptPlayable scriptPlayable;

        // Data used by the animation job
        protected NativeArray<float> boneWeights;
        protected NativeArray<TransformStreamHandle> boneHandles;

        #region Lifecycle methods
        public override void RuntimeInitialize(int startNodeIndex)
        {
            base.RuntimeInitialize(startNodeIndex);
            
            sharedData = stateMachineGraph.AnimationLayerStartNodes.FirstOrDefault().SharedData;

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

            scriptPlayable = InitializePlayable(); //AnimationScriptPlayable.Create(sharedData.PlayableGraph, job);
            // scriptPlayable.SetProcessInputs(false);
        }

        private void OnDestroy()
        {
            boneHandles.Dispose();
            boneWeights.Dispose();
        }

        public override void Enter()
        {
            base.Enter();
            
            Debug.Log($"ENTER {base.name}");
            
            sharedData.GetAnimationLayer(StartNodeIndex).TransitionToState(this);
        }
        
        public override void Exit()
        {
            base.Exit();

            Debug.Log($"EXIT {base.name}");
        }

        public override void Execute()
        {
            base.Execute();

            UpdateWeights();
        }
        #endregion

        protected abstract AnimationScriptPlayable InitializePlayable();

        private void UpdateWeights()
        {
            for (var i = 0; i < boneTransformWeights.Length; ++i)
            {
                var boneWeight = boneTransformWeights[i].weight;
                var childrenIndices = boneChildrenIndices[i];
                foreach (var index in childrenIndices)
                    boneWeights[index] = boneWeight;
            }
        }
    }
}