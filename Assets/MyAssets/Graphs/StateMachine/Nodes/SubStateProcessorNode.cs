using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public class SubStateProcessorNode : StateNode
{
    [ValidateInput("ValidateInput",
        "You added a SubStateProcessorNode to the list! Do you want infinite loop?")]
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [ListDrawerSettings(Expanded = true)]
    private List<StateNode> subStates;
    
    #region LifeCycle Methods

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
        subStates.ForEach(s => s.Initialize(parentGraph));
    }

    public override void RuntimeInitialize()
    {
        base.RuntimeInitialize();
        subStates.ForEach(s => s.RuntimeInitialize());
    }

    public override void Enter()
    {
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
}
