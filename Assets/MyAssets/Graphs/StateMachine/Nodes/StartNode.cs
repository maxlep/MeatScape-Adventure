using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class StartNode : Node
{
    [Output] public StateNode EntryState;
    
    [InfoBox("You connected >1 entry state to this node! Please only connect 1 at most.", InfoMessageType.Error,
        "HasMultipleConnection")] [LabelWidth(135f)]
    [SerializeField] private int executionOrderIndex = 0;

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

    private void OnValidate()
    {
        name = $"Start {executionOrderIndex}";
    }
}
