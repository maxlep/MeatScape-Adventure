using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class StartNode : Node
{
    [Output (connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public StateMachineConnection EntryState;
    
    [InfoBox("You connected >1 entry state to this node! Please only connect 1 at most.", InfoMessageType.Error,
        "HasMultipleConnection")] [LabelWidth(165f)]
    [SerializeField] protected int executionOrderIndex = 0;

    protected StateMachineGraph stateMachineGraph;
    
    public int ExecutionOrderIndex
    {
        get => executionOrderIndex;
    }
    private bool HasMultipleConnection(int dummy)
    {
        if (GetOutputPort("EntryState").GetConnections().Count > 1)
            return true;
        
        return false;
    }

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        stateMachineGraph = parentGraph;
    }

    public virtual void RuntimeInitialize()
    {
        
    }

    protected virtual void OnValidate()
    {
        name = $"Start {executionOrderIndex}";
    }
    
    public virtual void OnApplictionExit()
    {
    }
}
