using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using XNode;

public class StateReferenceNode : Node
{
    [Input] [PropertyOrder(-3)] public StateNode previousState;

    [ValueDropdown("GetStateNodes")]
    [SerializeField] [HideLabel] private StateNode referencedNode;
    
    private StateMachineGraph stateMachineGraph;

    public StateNode ReferencedNode => referencedNode;
    
    
    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        PopulateLinkedNodes();
    }

    private void OnValidate()
    {
        PopulateLinkedNodes();
    }

    private void PopulateLinkedNodes()
    {
        linkedNodes.Clear();
        if (referencedNode != null)
        {
            linkedNodes.Add(referencedNode);
        }
        foreach (var stateRefNode in stateMachineGraph.StateReferenceNodes)
        {
            if (stateRefNode.ReferencedNode == referencedNode)
                linkedNodes.Add(stateRefNode);
        }
    }

    private  List<StateNode> GetStateNodes()
    {
        return stateMachineGraph.stateNodes;
    }
}
