using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.Runtime.AnimationJobs;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Debug = UnityEngine.Debug;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class NewSequenceBlend : AnimationStateNode
    {
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected FloatReference factor;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected ExtrapolateBehavior extrapolateMode;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected SequenceUnit[] sequence;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected bool loopSequence = true;

        protected override AnimationScriptPlayable InitializePlayable()
        {
            // Create job
            var job = new SequenceBlendJob()
            {
                handles = boneHandles,
                boneWeights = boneWeights,
                weight = 0f,
                poseAIndex = 0,
                poseBIndex = 1
            };

            var scriptPlayable = AnimationScriptPlayable.Create(sharedData.PlayableGraph, job);
            scriptPlayable.SetProcessInputs(false);
            sequence.ForEach(clip =>
            {
                scriptPlayable.AddInput(AnimationClipPlayable.Create(sharedData.PlayableGraph, clip.pose), 0, 1.0f);
            });

            return scriptPlayable;
        }

        private int blendTweenId;

        public override void Execute()
        {
            base.Execute();

            var job = scriptPlayable.GetJobData<SequenceBlendJob>();

            var sequenceIndices = GetTransitionIndices();
            
            Debug.Log(boneWeights);

            job.weight = sequenceIndices.weight;
            job.poseAIndex = sequenceIndices.start;
            job.poseBIndex = sequenceIndices.end;
            job.boneWeights = boneWeights;

            scriptPlayable.SetJobData(job);
        }

        // private void OnDisable()
        // {
        //     Debug.Log($"DISABLE" + m_Graph.GetEditorName());
        //     m_Graph?.Stop();
        // }

        (int start, int end, float weight) GetTransitionIndices()
        {
            var numPoses = sequence.Length;

            var poseSize = 1.0f / (loopSequence ? numPoses : numPoses - 1);

            Debug.Assert(!float.IsPositiveInfinity(Mathf.Abs(factor.Value)));
            var poseNum = factor.Value / poseSize;
            // Debug.Log($"Blend: {numPoses}, {poseSize}, {factor.Value}, {poseNum}");
            int start, end;
            float transitionWeight;
            switch (extrapolateMode)
            {
                case ExtrapolateBehavior.Hold:
                    start = Mathf.FloorToInt(Mathf.Clamp(poseNum, 0, numPoses - 1));
                    end = Mathf.Min(start + 1, numPoses - 1);
                    transitionWeight = factor.Value / poseSize;
                    break;
                case ExtrapolateBehavior.Overshoot:
                case ExtrapolateBehavior.PingPong:
                    throw new NotImplementedException();
                default:
                case ExtrapolateBehavior.Wrap:
                    start = Mathf.FloorToInt(poseNum % numPoses);
                    end = (start + 1) % numPoses;
                    transitionWeight = (factor.Value % poseSize) / poseSize;
                    break;
            }

            var interpWeight = sequence[start].transition.Evaluate(transitionWeight);

            return (start, end, interpWeight);
        }
    }
}
