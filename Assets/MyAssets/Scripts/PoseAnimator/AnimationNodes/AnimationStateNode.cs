using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MyAssets.Runtime.AnimationJobs;
using MyAssets.Scripts.PoseAnimator.Components;
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

        protected Animatable _animatable;

        #region Lifecycle methods
        public override void RuntimeInitialize(int startNodeIndex)
        {
            base.RuntimeInitialize(startNodeIndex);

            _animatable = stateMachineGraph.AnimationLayerStartNodes.FirstOrDefault().Animatable;
            // return;
        }

        #endregion
    }
}