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
        [LabelWidth(165f)] public string Name; 
        public Playable Output => mixerRunner.Output;
        
        private SharedAnimationData sharedData;
        
        [SerializeField] [LabelWidth(165f)]
        private AnimationClip defaultPose;

        [ShowInInspector] [LabelWidth(165f)] [DisableIf("AlwaysTrue")] 
        private AnimationStateNode activeState, nextActiveState;

        [SerializeField] [InlineProperty] [HideLabel] 
        [BoxGroup("MixerRunner")] [PropertySpace(10f, 0f)]
        private MixerRunner mixerRunner;

        private AnimationClipPlayable defaultPlayable;
        
        private LTDescr transitionTween;
        private bool transitioning;
        private bool AlwaysTrue => true;

        public AnimationLayer(string name, SharedAnimationData sharedData)
        {
            Name = name;
            this.sharedData = sharedData;

            // Debug.Log($"Init animation layer, {Name}");
            // mixerPlayable = AnimationMixerPlayable.Create(sharedData.PlayableGraph, 2);
            defaultPlayable = AnimationClipPlayable.Create(this.sharedData.PlayableGraph, defaultPose);
            
            mixerRunner = new MixerRunner();
        }

        public void Initialize()
        {
            mixerRunner.Initialize(Name, sharedData);
        }

        public void Update()
        {
            mixerRunner.Update();
        }
        
        public void TransitionToState(AnimationStateNode newState, float transitionTime, AnimationCurve transitionCurve)
        {
            if (transitioning)
            {
                transitionTween.callOnCompletes();
                // transitionTween.setOnUpdate((float value) => { }).setOnComplete(() => { });
                LeanTween.cancel(transitionTween.id);
            }
            
            mixerRunner.Output.DisconnectInput(0);
            mixerRunner.Output.DisconnectInput(1);
            // if (!mixerPlayable.GetInput(0).IsNull()) mixerPlayable.DisconnectInput(0);
            // if (!mixerPlayable.GetInput(1).IsNull()) mixerPlayable.DisconnectInput(1);
            
            activeState = nextActiveState;
            nextActiveState = newState;
            
            // Debug.Log($"Transition from {activeState?.name} to {nextActiveState?.name}, {newState.Output.IsValid()}, {newState.Output.GetInputCount()}");
            
            if (nextActiveState) mixerRunner.Output.ConnectInput(0, nextActiveState.Output, 0);
            else mixerRunner.Output.ConnectInput(0, defaultPlayable, 0);
            
            if (activeState) mixerRunner.Output.ConnectInput(1, activeState.Output, 0);
            
            mixerRunner.Output.SetInputWeight(0, 1f);
            mixerRunner.Output.SetInputWeight(1, 1f);

            if (nextActiveState != null)
            {
                transitioning = true;
                mixerRunner.SetBlendFromLastPosition(true);

                transitionTween =
                    LeanTween
                        .value(sharedData.Animatable.RootTransform.gameObject, 0f, 1f, transitionTime)
                        .setEase(transitionCurve)
                        .setOnStart(() =>
                        {
                            // transitioning = true;
                            // mixerRunner.SetBlendFromLastPosition(true);
                            //Debug.Log($"Start tween from {activeState?.name} to {nextActiveState?.name}");
                        })
                        .setOnUpdate((float value) =>
                        {
                            mixerRunner.Factor = value;
                            //Debug.Log($"Update tween value {mixerRunner.Factor}");
                        })
                        .setOnComplete(() =>
                        {
                            transitioning = false;
                            mixerRunner.SetBlendFromLastPosition(false);
                            //Debug.Log($"Complete tween from {activeState?.name} to {nextActiveState?.name}");
                        });
            }

            // sharedData.Animatable.playableOutput.SetSourcePlayable(newState.Output);

            // mixerRunner.Factor = 0.0f;
        }

        public void Dispose()
        {
            mixerRunner.Dispose();
            defaultPlayable.Destroy();
        }
    }
}