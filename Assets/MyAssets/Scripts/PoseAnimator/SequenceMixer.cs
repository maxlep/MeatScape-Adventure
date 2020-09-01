using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.Scripts.PoseAnimator;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;



public class SequenceMixer : MonoBehaviour
{
    [SerializeField] [Range(0.0f, 1.0f)] protected float weight = 1.0f;
    [SerializeField] private string fbxName;
    [SerializeField] protected List<string> clipNames;
    [SerializeField] private AnimationCurve[] transitionCurves;

    public BoneTransformWeight[] boneTransformWeights;

    List<List<int>> m_BoneChildrenIndices;

    NativeArray<TransformStreamHandle> m_Handles;
    NativeArray<float> m_BoneWeights;

    PlayableGraph m_Graph;
    AnimationScriptPlayable m_CustomMixerPlayable;

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

    void OnEnable()
    {
        // Load animation clips.
        var poses = clipNames.Select(clip => SampleUtility.LoadAnimationClipFromFbx(fbxName, clip)).ToList();
        if (!poses.Any() || poses.Any(pose => pose == null))
            return;

        var animator = GetComponent<Animator>();

        // Get all the transforms in the hierarchy.
        var allTransforms = animator.transform.GetComponentsInChildren<Transform>();
        var numTransforms = allTransforms.Length - 1;

        // Fill native arrays (all the bones have a weight of 0.0).
        m_Handles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        m_BoneWeights = new NativeArray<float>(numTransforms, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        for (var i = 0; i < numTransforms; ++i)
            m_Handles[i] = animator.BindStreamTransform(allTransforms[i + 1]);

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
        m_Graph = PlayableGraph.Create("SequenceMixer");
        m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        m_CustomMixerPlayable = AnimationScriptPlayable.Create(m_Graph, job);
        m_CustomMixerPlayable.SetProcessInputs(false);
        poses.ForEach(pose =>
        {
            m_CustomMixerPlayable.AddInput(AnimationClipPlayable.Create(m_Graph, pose), 0, 1.0f);
        });

        var output = AnimationPlayableOutput.Create(m_Graph, "output2", animator);
        output.SetSourcePlayable(m_CustomMixerPlayable);

        m_Graph.Play();
    }

    protected void Update()
    {
        var job = m_CustomMixerPlayable.GetJobData<SequenceBlendJob>();

        UpdateWeights();
        var sequenceIndices = GetTransitionIndices();
        // var interp = transitionCurves[sequenceIndices.start].Evaluate(weight);

        job.weight = sequenceIndices.weight;
        job.poseAIndex = sequenceIndices.start;
        job.poseBIndex = sequenceIndices.end;
        job.boneWeights = m_BoneWeights;

        m_CustomMixerPlayable.SetJobData(job);
    }

    void OnDisable()
    {
        m_Graph.Destroy();
        m_Handles.Dispose();
        m_BoneWeights.Dispose();
    }
    
    private (int start, int end, float weight) GetTransitionIndices()
    {
        var numPoses = clipNames.Count;
        var stepSize = 1.0f / numPoses;

        var start = Mathf.FloorToInt(Mathf.Clamp(weight / stepSize, 0, numPoses - 1));
        var wrap = start == (numPoses - 1);
        var end = wrap ? 0 : start + 1;

        var transitionWeight = (weight % stepSize) / stepSize;
        var interpWeight = transitionCurves[start].Evaluate(transitionWeight);
        
        return (start, end, interpWeight);
    }
}
