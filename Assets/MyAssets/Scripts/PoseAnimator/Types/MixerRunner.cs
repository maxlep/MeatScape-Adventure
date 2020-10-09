using System;
using MyAssets.Runtime.AnimationJobs;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyAssets.Scripts.PoseAnimator.Types
{
    [Serializable]
    public class MixerRunner : AnimationJobRunner
    {
        [LabelWidth(165f)] public float Factor;
        [LabelWidth(165f)] public BlendMode BlendMode;
        [HideInInspector] public NativeArray<BoneLocation> BonesLastFrame;

        private bool blendFromLastPosition, resetTransitioning;

        protected override AnimationScriptPlayable InitializePlayable()
        {
            BonesLastFrame = sharedData.GetBonesLastFrameCopy();
            
            // Create job
            var job = new MixerJob()
            {
                blendMode = BlendMode,
                handles = boneHandles,
                boneWeights = boneWeights,
                bonesLastFrame = BonesLastFrame,
                bonesLastState = BonesLastFrame,
                weight = 0f,
            };
            
            // Debug.Log($"Initialize mixer playable {job}");

            var scriptPlayable = AnimationScriptPlayable.Create(sharedData.PlayableGraph, job, 2);
            scriptPlayable.SetProcessInputs(false);
            // sequence.ForEach(clip =>
            // {
            //     scriptPlayable.AddInput(AnimationClipPlayable.Create(sharedData.PlayableGraph, clip.pose), 0, 0f);
            // });

            return scriptPlayable;
        }

        protected override void UpdateJob()
        {
            var job = scriptPlayable.GetJobData<MixerJob>();

            // Debug.Log($"Update mixer job {scriptPlayable.IsValid()}|{job.weight}|{Factor}");
            job.weight = Factor;
            job.boneWeights = boneWeights;
            job.useBonesLastAsFirst = blendFromLastPosition;
            if (resetTransitioning)
            {
                resetTransitioning = false;
                job.isTransitioning = false;
            }

            scriptPlayable.SetJobData(job);
        }

        public override void Dispose()
        {
            base.Dispose();

            BonesLastFrame.Dispose();
        }

        public void SetBlendFromLastPosition(bool value)
        {
            // Debug.LogWarning($"Mixer runner set blend transition {value}");
            blendFromLastPosition = value;
            if (!blendFromLastPosition) resetTransitioning = true;
        }
    }
}