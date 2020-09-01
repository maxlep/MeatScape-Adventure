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
    
    private List<TransitionNode> transitionNodes = new List<TransitionNode>();
    protected StateNode noTransitionState;
    private StateMachineGraph stateMachineGraph;

    public StateNode ReferencedState => referencedState;
    
    public List<StateNode> previousStates { get; private set; } = new List<StateNode>();
    public List<StateNode> nextStates { get; private set; } = new List<StateNode>();
    
    
    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        PopulateLinkedNodes();
        PopulateTransitionNodeList();
        PopulatePreviousStates();
        PopulateNextStates();
    }

    private void PopulateLinkedNodes()
    {
        linkedNodes.Clear();
        if (referencedState != null)
        {
            linkedNodes.Add(referencedState);
        }
        foreach (var stateRefNode in stateMachineGraph.StateReferenceNodes)
        {
            if (stateRefNode.ReferencedState == referencedState)
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
    
    private void PopulatePreviousStates()
    {
        previousStates.Clear();

        NodePort transitionsPort = GetInputPort("previousState");
        transitionsPort.GetConnections().ForEach(c =>
        {
            TransitionNode nodeAsTransition = (c.node as TransitionNode);
            if (nodeAsTransition != null)
            {
                previousStates.Add(nodeAsTransition.GetStartingState());
            }
                
            
            StateNode nodeAsState = (c.node as StateNode);
            if (nodeAsState != null)
                previousStates.Add(nodeAsState);
        });
    }
    
    private void PopulateNextStates()
    {
        nextStates.Clear();
        
        if (noTransitionState != null)
            nextStates.Add(noTransitionState);

        foreach (var transitionNode in transitionNodes)
        {
            StateNode transState = transitionNode.GetNextState();
            if (transState != null)
                nextStates.Add(transState);
        }
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
