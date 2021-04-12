using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;

public class SubStateProcessorNode : StateNode
{
    [HideIf("$collapsed")] [SerializeField] [Required]
    protected SubStateGraph subGraph;
    
    [ValidateInput("ValidateInput",
        "You added a SubStateProcessorNode to the list! Do you want infinite loop?")]
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [PropertyOrder(1)]
    [ListDrawerSettings(Expanded = true, IsReadOnly = true)] [Sirenix.OdinInspector.ReadOnly]
    protected List<StateNode> subStates = new List<StateNode>();

    public List<StateNode> SubStates => subStates;
    
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

    public override void OnApplictionExit()
    {
        base.OnApplictionExit();
        subStates.ForEach(s => s.OnApplictionExit());
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        PopulateSubStates();
    }

    #endregion

    private void PopulateSubStates()
    {
        if (subGraph == null)
            return;
        
        subStates.Clear();

        foreach (var node in subGraph.nodes)
        {
            if (node is StateNode)
            {
                StateNode nodeAsState = (StateNode) node;
                subStates.Add(nodeAsState);
            }
        }
    }

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
