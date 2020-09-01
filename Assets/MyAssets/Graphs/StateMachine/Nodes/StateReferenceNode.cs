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
    [SerializeField] [HideLabel] private StateNode referencedNode;
    
    private List<TransitionNode> transitionNodes = new List<TransitionNode>();
    protected StateNode noTransitionState;
    private StateMachineGraph stateMachineGraph;

    public StateNode ReferencedNode => referencedNode;
    
    
    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        PopulateLinkedNodes();
        PopulateTransitionNodeList();
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
    
    private void PopulateTransitionNodeList()
    {
        transitionNodes.Clear();

        NodePort transitionsPort = GetOutputPort("transitions");
        transitionsPort.GetConnections().ForEach(c =>
        {
            TransitionNode nodeAsTransition = (c.node as TransitionNode);
            if (nodeAsTransition != null)
                transitionNodes.Add(c.node as TransitionNode);
            
            StateNode nodeAsState = (c.node as StateNode);
            if (nodeAsState != null)
                noTransitionState = nodeAsState;
        });
    }
    
    public virtual StateNode CheckStateTransitions(TriggerVariable receivedTrigger = null)
    {
        //Check for direct connection to state that bypasses transtions
        if (noTransitionState != null) return noTransitionState;
        
        //Check global transitions
        foreach (var transition in stateMachineGraph.globalTransitions)
        {
            if (transition.EvaluateConditions(receivedTrigger))
            {
                return transition.GetNextState();
            }
        }
        
        //Check connected transitions
        foreach (var transition in transitionNodes)
        {
            if (transition.EvaluateConditions(receivedTrigger))
            {
                return transition.GetNextState();
            }
        }

        return null;
    }

    private  List<StateNode> GetStateNodes()
    {
        return stateMachineGraph.stateNodes;
    }
}
