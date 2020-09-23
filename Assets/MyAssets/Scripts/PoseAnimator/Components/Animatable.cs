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

        public SharedAnimationData SharedData => sharedData;
        [ShowInInspector] private SharedAnimationData sharedData;

        private AnimationLayerMixerPlayable layerMixerPlayable;
        public AnimationPlayableOutput playableOutput;

        private void Awake()
        {
            var playableGraph = PlayableGraph.Create(name);
            
            sharedData = new SharedAnimationData(animator, playableGraph);

            animationStateMachine.AnimationLayerStartNodes.ForEach((i, index) =>
                i.InjectAnimationLayer(new AnimationLayer(sharedData)));

            var startIndexToLayerIndex = new Dictionary<int, int>();
            var animationLayers =
                animationStateMachine
                    .AnimationLayerStartNodes
                    .Select((i, index) =>
                    {
                        startIndexToLayerIndex[i.ExecutionOrderIndex] = index;
                        return i.AnimationLayer;
                    })
                    .ToList();

            sharedData.Animatable = this;

            sharedData.AnimationLayers = animationLayers;
            sharedData.StartIndexToLayerIndex = startIndexToLayerIndex;

            layerMixerPlayable = AnimationLayerMixerPlayable.Create(sharedData.PlayableGraph, animationLayers.Count);
            Debug.Log("Creating layer mixer");
            sharedData.AnimationLayers.ForEach((i, index) =>
            {
                Debug.Log($"Connect layer {index} {i.Output.IsNull()}");
                layerMixerPlayable.ConnectInput(index, i.Output, 0);
            });
            layerMixerPlayable.SetInputWeight(0, 1f);
            layerMixerPlayable.SetTraversalMode(PlayableTraversalMode.Passthrough);
            
            playableOutput = AnimationPlayableOutput.Create(sharedData.PlayableGraph,
                $"{sharedData.PlayableGraph.GetEditorName()}_Output", animator);
            playableOutput.SetSourcePlayable(layerMixerPlayable);

            animationStateMachine.InjectAnimatable(this);
            
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
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
    }
}