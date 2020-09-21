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
    public class SequenceBlend : PlayerStateNode
    {
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected float transitionTime = 1f;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected AnimationCurve transitionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected FloatReference factor;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected ExtrapolateBehavior extrapolateMode;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected SequenceUnit[] sequence;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected bool loopSequence = true;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] protected BoneTransformWeight[] boneTransformWeights;
    
        Animator animator;
        List<List<int>> m_BoneChildrenIndices;
        NativeArray<TransformStreamHandle> m_Handles;
        NativeArray<float> m_BoneWeights;
        PlayableGraph m_Graph;
        AnimationScriptPlayable m_CustomMixerPlayable;

        public override void RuntimeInitialize()
        {
            base.RuntimeInitialize();

            if (!sequence.Any() || sequence.Any(pose => pose.pose == null))
                return;

            animator = playerController.GetAnimator();
        
            // Get all the transforms in the hierarchy.
            var allTransforms = animator.transform.GetComponentsInChildren<Transform>();
            var numTransforms = allTransforms.Length - 1;
        
            // Fill native arrays (all the bones have a weight of 0.0).
            m_Handles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            m_BoneWeights = new NativeArray<float>(numTransforms, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            for (var i = 0; i < numTransforms; ++i)
                m_Handles[i] = animator.BindStreamTransform(allTransforms[i + 1]);
        
            // Load selected bones
            var root = new BoneTransformWeight() {transform = playerController.GetRoot(), weight = 1.0f};
            boneTransformWeights = new[] { root };
        
            // Set bone weights for selected transforms and their hierarchy.
            m_BoneChildrenIndices = new List<List<int>>(boneTransformWeights.Length);
            foreach (var boneTransform in boneTransformWeights)
            {
                var childrenTransforms = boneTransform.transform.GetComponentsInChildren<Transform>();
                var childrenIndices = new List<int>(childrenTransforms.Length);
                foreach (var childTransform in childrenTransforms)
                {
                    var boneIndex = Array.IndexOf(allTransforms, childTransform);
                    Debug.Assert(boneIndex > 0, "Index can't be less or equal to 0");
                    childrenIndices.Add(boneIndex - 1);
                }
        
                m_BoneChildrenIndices.Add(childrenIndices);
            }
        
            // Create job
            var job = new SequenceBlendJob()
            {
                handles = m_Handles,
                boneWeights = m_BoneWeights,
                weight = 0f,
                poseAIndex = 0,
                poseBIndex = 1
            };
        
            // Create graph with custom mixer.
            // m_Graph = PlayableGraph.Create(base.name);
            m_Graph = StateTransitionMixer.Instance.GetGraph();
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            m_CustomMixerPlayable = AnimationScriptPlayable.Create(m_Graph, job);
            m_CustomMixerPlayable.SetProcessInputs(false);
            sequence.ForEach(clip =>
            {
                m_CustomMixerPlayable.AddInput(AnimationClipPlayable.Create(m_Graph, clip.pose), 0, 1.0f);
            });

            // var testPlayable = AnimationLayerMixerPlayable.Create(m_Graph);
            // m_CustomMixerPlayable.ConnectInput();
        
            // var output = AnimationPlayableOutput.Create(m_Graph, $"{base.name}_Output", animator);
            // output.SetSourcePlayable(m_CustomMixerPlayable);
        
            // m_Graph.Play();
        }

        private int blendTweenId;
    
        public override void Enter()
        {
            base.Enter();
        
            Debug.Log($"ENTER {name} {m_Graph.GetEditorName()}");
            StateTransitionMixer.Instance.Blend(m_CustomMixerPlayable, transitionTime, transitionCurve);

            // LeanTween.value(playerController.gameObject, (value) =>
            // {
            //     boneTransformWeights[0].weight = value;
            // }, 0f, 1f, 1f);

            // m_Graph.Play();
        }

        public override void Exit()
        {
            base.Exit();
        
            // Debug.Log($"EXIT" + m_Graph.GetEditorName());
            // m_Graph.Stop();
        }

        public override void Execute()
        {
            base.Execute();

            var job = m_CustomMixerPlayable.GetJobData<SequenceBlendJob>();
        
            UpdateWeights();
            var sequenceIndices = GetTransitionIndices();
        
            job.weight = sequenceIndices.weight;
            job.poseAIndex = sequenceIndices.start;
            job.poseBIndex = sequenceIndices.end;
            job.boneWeights = m_BoneWeights;
        
            m_CustomMixerPlayable.SetJobData(job);
        }

        // private void OnDisable()
        // {
        //     Debug.Log($"DISABLE" + m_Graph.GetEditorName());
        //     m_Graph?.Stop();
        // }

        private void OnDestroy()
        {
            m_Graph.Destroy();
            m_Handles.Dispose();
            m_BoneWeights.Dispose();
        }

        void UpdateWeights()
        {
            for (var i = 0; i < boneTransformWeights.Length; ++i)
            {
                var boneWeight = boneTransformWeights[i].weight;
                var childrenIndices = m_BoneChildrenIndices[i];
                foreach (var index in childrenIndices)
                    m_BoneWeights[index] = boneWeight;
            }
        }

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
