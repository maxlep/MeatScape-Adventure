﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.PoseAnimator;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class StateTransitionMixer : PlayerStateNode
{
    public PlayableGraph GetGraph() => playableGraph;

    PlayableGraph playableGraph;
    AnimationMixerPlayable mixerPlayable;
    
    AnimationScriptPlayable activePlayable;
    AnimationScriptPlayable tweenPlayable;
    bool tweening = false;
    private LTDescr lastTween;
    AnimationPlayableOutput playableOutput;

    public float defaultTweenDuration = 0.5f;
    private float weight;

    public static StateTransitionMixer Instance;

    public override void RuntimeInitialize()
    {
        base.RuntimeInitialize();

        if (Instance == null) Instance = this;
        else Destroy(this);
        
        // Creates the graph, the mixer and binds them to the Animator.
        playableGraph = PlayableGraph.Create();
        playableOutput = AnimationPlayableOutput.Create(playableGraph, $"{base.name}", playerController.GetAnimator());
        activePlayable = AnimationScriptPlayable.Create(playableGraph, new NoOpJob());
        playableOutput.SetSourcePlayable(activePlayable);
        playableGraph.Play();
    }

    public override void Enter()
    {
        base.Enter();
    }
    
    public void Blend(AnimationScriptPlayable playable, float duration, AnimationCurve transitionCurve)
    {
        if (tweening && lastTween != null)
        {
            lastTween.callOnCompletes();
            LeanTween.cancel(lastTween.id);
        }
        
        // tweenPlayable = AnimatorControllerPlayable.Create(playableGraph, ac);
        tweenPlayable = playable;
        mixerPlayable  = AnimationMixerPlayable.Create(playableGraph, 2);

        mixerPlayable.ConnectInput(0, activePlayable, 0);
        mixerPlayable.ConnectInput(1, tweenPlayable, 0);

        // Plays the Graph.
        mixerPlayable.SetInputWeight(0, 1);
        playableOutput.SetSourcePlayable(mixerPlayable);

        lastTween = LeanTween
            .value(playerController.gameObject, 0f, 1f, duration)
            .setEase(transitionCurve)
            .setOnUpdate((float value) =>
            {
                mixerPlayable.SetInputWeight(0, 1f - value);
                mixerPlayable.SetInputWeight(1, value);
            })
            .setOnComplete(() =>
            {
                tweening = false;
                
                playableGraph.Disconnect(mixerPlayable, 0);
                playableGraph.Disconnect(mixerPlayable, 1);
                playableOutput.SetSourcePlayable(tweenPlayable);
                var prevActive = activePlayable;
                activePlayable = tweenPlayable;
                // prevActive.Destroy();
                mixerPlayable.Destroy();
            });

        tweening = true;
    }

    public override void Execute()
    {
        base.Execute();
    }

    // void OnDisable()
    // {
    //     // Destroys all Playables and Outputs created by the graph.
    //     playableGraph.Destroy();
    // }
}