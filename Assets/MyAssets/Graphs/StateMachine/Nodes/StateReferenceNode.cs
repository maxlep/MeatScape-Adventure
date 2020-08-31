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
    }

    private void OnValidate()
    {
        if (referencedNode != null)
        {
            linkedNodes.Clear();
            linkedNodes.Add(referencedNode);
        }
    }

    private  List<StateNode> GetStateNodes()
    {
        return stateMachineGraph.stateNodes;
    }
}
