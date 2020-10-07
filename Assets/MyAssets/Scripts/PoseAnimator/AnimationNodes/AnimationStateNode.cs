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
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        private float transitionTime;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        private AnimationCurve transitionCurve;

        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        private bool isStateOutput = true;

        public virtual Playable Output => Playable.Null;
        
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
            
            // Debug.Log($"ENTER {base.name}");
            
            if (isStateOutput) sharedData.GetAnimationLayer(StartNodeIndex).TransitionToState(this, transitionTime, transitionCurve);
        }
        
        public override void Exit()
        {
            base.Exit();

            // Debug.Log($"EXIT {base.name}");
        }

        #endregion
    }
}