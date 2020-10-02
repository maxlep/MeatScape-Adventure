using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using MyAssets.Scripts.PoseAnimator.Types;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyAssets.Scripts.PoseAnimator.Components
{
    public class Animatable : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private StateMachineGraph animationStateMachine;

        public Transform RootTransform => rootTransform.Value;
        [SerializeField] private TransformSceneReference rootTransform;

        public SharedAnimationData SharedData => sharedData;
        [ShowInInspector] private SharedAnimationData sharedData;

        private AnimationLayerMixerPlayable layerMixerPlayable;
        public AnimationPlayableOutput playableOutput;

        private void Awake()
        {
            var playableGraph = PlayableGraph.Create(name);
            
            sharedData = new SharedAnimationData(animator, playableGraph);

            // var startIndexToLayerIndex = new Dictionary<int, int>();
            // var animationLayers =
            //     animationStateMachine
            //         .AnimationLayerStartNodes
            //         .Select((i, index) =>
            //         {
            //             startIndexToLayerIndex[i.ExecutionOrderIndex] = index;
            //             return i.AnimationLayer;
            //         })
            //         .ToList();

            sharedData.Animatable = this;

            sharedData.AnimationLayers = new List<AnimationLayer>();
            sharedData.StartIndexToLayerIndex = new Dictionary<int, int>();

            layerMixerPlayable = AnimationLayerMixerPlayable.Create(sharedData.PlayableGraph);
            // Debug.Log("Creating layer mixer");
            // sharedData.AnimationLayers.ForEach((i, index) =>
            // {
            //     Debug.Log($"Connect layer {index} {i.Output.IsNull()}");
            //     layerMixerPlayable.ConnectInput(index, i.Output, 0);
            // });
            layerMixerPlayable.SetTraversalMode(PlayableTraversalMode.Passthrough);
            
            playableOutput = AnimationPlayableOutput.Create(sharedData.PlayableGraph,
                $"{sharedData.PlayableGraph.GetEditorName()}_Output", animator);
            playableOutput.SetSourcePlayable(layerMixerPlayable);

            // animationStateMachine.InjectAnimatable(this);
            
            // playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            playableGraph.Play();
        }

        private void Update()
        {
            sharedData.AnimationLayers.ForEach(i => i.Update());
        }

        private void OnDestroy()
        {
            sharedData.Dispose();
        }

        public void RegisterAnimationLayer(AnimationLayerStartNode animationLayerStartNode)
        {
            sharedData.StartIndexToLayerIndex[animationLayerStartNode.ExecutionOrderIndex] =
                sharedData.AnimationLayers.Count;
            sharedData.AnimationLayers.Add(animationLayerStartNode.AnimationLayer);

            layerMixerPlayable.AddInput(animationLayerStartNode.AnimationLayer.Output, 0, 1f);

            // Debug.Log(
            //     $"Registered animation layer {animationLayerStartNode.name} {sharedData.AnimationLayers.Count} {sharedData.StartIndexToLayerIndex.Count}");
        }
    }
}