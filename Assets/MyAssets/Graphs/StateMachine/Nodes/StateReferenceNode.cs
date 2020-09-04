using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using XNode;

public class StateReferenceNode : Node
{
    [Input] [PropertyOrder(-3)] public StateNode previousState;
    [Output] [PropertyOrder(-2)]  public TransitionNode transitions;


    [ValueDropdown("GetStateNodes")]
    [SerializeField] [Required] [HideLabel] private StateNode referencedState;
    
    protected bool isInitialized = false;
    private StateMachineGraph stateMachineGraph;

    public StateNode ReferencedState => referencedState;
    
    public bool IsInitialized
    {
        get => isInitialized;
        set => isInitialized = value;
    }

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        isInitialized = true;
        PopulateLinkedNodes();
    }
    
    public virtual void RuntimeInitialize()
    {
        
    }
    
    private  List<StateNode> GetStateNodes()
    {
        Debug.Log($"{stateMachineGraph.name} has state nodes: {stateMachineGraph.stateNodes.Count}");
        return stateMachineGraph.stateNodes;
    }

    private void PopulateLinkedNodes()
    {
        linkedNodes.Clear();
        
        //Store link to referenced state
        if (referencedState != null)
        {
            linkedNodes.Add(referencedState);
        }
        
        //Store link to each ref node that points to same state
        foreach (var stateRefNode in stateMachineGraph.StateReferenceNodes)
        {
            if (stateRefNode.ReferencedState == referencedState)
                linkedNodes.Add(stateRefNode);
        }
    }
    
    private void OnValidate()
    {
        name = $"{referencedState.GetName()} <Reference>";
    }


}
