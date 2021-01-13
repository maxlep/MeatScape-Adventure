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
    public class AnimSubStateProcessorNode : StateNode
    {
        [ValidateInput("ValidateOutputState", 
            "Output state must be an animation state node present in subState List!")]
        [PropertySpace(0f, 10f)] [SerializeField]
        private StateNode outputState;
        
        [ValidateInput("ValidateInput",
            "You added a SubStateProcessorNode to the list! Do you want infinite loop?")]
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [ListDrawerSettings(Expanded = true)] [Required]
        private List<StateNode> subStates = new List<StateNode>();

        private bool AlwaysTrue => true;
        
        #region LifeCycle Methods
        
        public override void Initialize(StateMachineGraph parentGraph)
        {
            base.Initialize(parentGraph);
            subStates.ForEach(s => s.Initialize(parentGraph));
        }
    
        public override void RuntimeInitialize(int startNodeIndex)
        {
            base.RuntimeInitialize(startNodeIndex);
            subStates.ForEach(s => s.RuntimeInitialize(startNodeIndex));
        }
    
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
            subStates.ForEach(s => s.Enter());
        }
    
        public override void Execute()
        {
            base.Execute();
            subStates.ForEach(s => s.Execute());
        }
    
        public override void ExecuteFixed()
        {
            base.ExecuteFixed();
            subStates.ForEach(s => s.ExecuteFixed());
        }
    
        public override void Exit()
        {
            base.Exit();
            subStates.ForEach(s => s.Exit());
        }
        
        public override void DrawGizmos()
        {
            base.DrawGizmos();
            subStates.ForEach(s => s.DrawGizmos());
        }
        
        #endregion
            
        private bool ValidateInput(List<StateNode> stateList)
        {
            //Dont allow adding SubStateProcessorNode to the list
            foreach (var state in stateList)
            {
                SubStateProcessorNode stateAsSubstateProcessor = state as SubStateProcessorNode;
                if (stateAsSubstateProcessor != null)
                    return false;
            }
        
            return true;
        }
        
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