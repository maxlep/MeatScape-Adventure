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
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        private float transitionTime;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), PropertySpace(0f, 10f), SerializeField]
        private AnimationCurve transitionCurve;

        private bool isStateOutput = true;

        public virtual Playable Output => Playable.Null;
        public void SetIsStateOutput(bool value) => isStateOutput = value;

        protected SharedAnimationData sharedData;

        #region Lifecycle methods
        public override void RuntimeInitialize(int startNodeIndex)
        {
            base.RuntimeInitialize(startNodeIndex);
            
            sharedData = stateMachineGraph.AnimationLayerStartNodes.FirstOrDefault().SharedData;
        }

        public override void Enter()
        {
            base.Enter();
            if (isStateOutput) sharedData.GetAnimationLayer(StartNodeIndex).TransitionToState(this, transitionTime, transitionCurve);
            // Debug.Log($"State: {name} | isStateOutput: {isStateOutput}");
        }
        
        public override void Exit()
        {
            base.Exit();

            // Debug.Log($"EXIT {base.name}");
        }

        #endregion
    }
}