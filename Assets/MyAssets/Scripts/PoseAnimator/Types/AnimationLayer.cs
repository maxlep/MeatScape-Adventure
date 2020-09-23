using System;
using MyAssets.Runtime.AnimationJobs;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MyAssets.Scripts.PoseAnimator.Types
{
    [Serializable]
    public class AnimationLayer
    {
        public Playable Output => mixerPlayable;
        
        private SharedAnimationData sharedData;
        private AnimationMixerPlayable mixerPlayable;
        private AnimationScriptPlayable customMixerPlayable;
        [ShowInInspector] private AnimationStateNode activeState, nextActiveState;

        public AnimationLayer(SharedAnimationData sharedData)
        {
            this.sharedData = sharedData;

            Debug.Log($"Init animation layer, {sharedData}");
            mixerPlayable = AnimationMixerPlayable.Create(sharedData.PlayableGraph, 2);
            
            // var job = new MixerJob()
            // {
            //     handles = 
            // };
            //
            // customMixerPlayable = AnimationScriptPlayable.Create(sharedData.PlayableGraph, );
            
            Debug.Log(mixerPlayable.IsNull());
        }

        public void Update()
        {
            
        }
        
        public void TransitionToState(AnimationStateNode newState)
        {
            mixerPlayable.DisconnectInput(0);
            mixerPlayable.DisconnectInput(1);
            // if (!mixerPlayable.GetInput(0).IsNull()) mixerPlayable.DisconnectInput(0);
            // if (!mixerPlayable.GetInput(1).IsNull()) mixerPlayable.DisconnectInput(1);
            
            activeState = nextActiveState;
            nextActiveState = newState;
            
            Debug.Log($"Transition from {activeState?.name} to {nextActiveState?.name}");
            
            if (activeState) mixerPlayable.ConnectInput(0, activeState.Output, 0);
            if (nextActiveState) mixerPlayable.ConnectInput(1, nextActiveState.Output, 0);
            mixerPlayable.SetInputWeight(0, 0f);
            mixerPlayable.SetInputWeight(1, 1f);
            // sharedData.Animatable.playableOutput.SetSourcePlayable(newState.Output);
        }

        public void Dispose()
        {
            mixerPlayable.Destroy();
        }
    }
}