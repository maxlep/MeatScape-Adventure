using System;
using System.Collections.Generic;
using KinematicCharacterController;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class AnimSubStateProcessorNode : SubStateProcessorNode
    {
        [ValidateInput("ValidateOutputState", 
            "Output state must be an animation state node present in subState List!")]
        [PropertySpace(0f, 10f)] [SerializeField] [Required]
        private StateNode outputState;
        
        #region LifeCycle Methods

        public override void Enter()
        {
            //Check if its anim state node, if so, set is output state
            foreach (var subState in subStates)
            {
                AnimationStateNode subStateAsAnimState = subState as AnimationStateNode;
                if (subStateAsAnimState != null)
                {
                    subStateAsAnimState.SetIsStateOutput(subStateAsAnimState == outputState);
                    // Debug.Log($"State: {subStateAsAnimState.name} | IsOutput: {subStateAsAnimState == outputState}");
                }
                
            }
            base.Enter();
        }

        #endregion
        
        
        private bool ValidateOutputState(StateNode outState)
        {
            if (subStates.IsNullOrEmpty()) return false;

            //Only allow animStateNodes as output
            AnimationStateNode outputAsAnimState = outState as AnimationStateNode;
            if (outputAsAnimState == null) return false;
            
            //Makes sure output state is present in list
            foreach (var state in subStates)
            {
                if (state == outState) return true;
            }

            return false;

        }
    }
}