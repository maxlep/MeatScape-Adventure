﻿using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu(fileName = "StateMachineGraph", menuName = "Graphs/StateMachineGraph", order = 0)]
public class StateMachineGraph : NodeGraph
{
    public LayeredStateMachine parentMachine { get; private set; }
    public List<StateNode> stateNodes { get; private set; } = new List<StateNode>();
    public List<TransitionNode> transitionNodes { get; private set; } = new List<TransitionNode>();
    public List<TransitionNode> globalTransitions { get; private set; } = new List<TransitionNode>();
    public StateNode currentState { get; private set; }
    
    private StartNode startNode;

    public void SetParentMachine(LayeredStateMachine parent) => parentMachine = parent;
    
    //For call by layered state machine to begin
    public void StartStateMachine()
    {
        InitStateNodes();
        InitTransitionNodes();
        EnterStartState();
    }

    public void ExecuteUpdates()
    {
        CheckForValidTransitions();
        currentState.Execute();
    }
        
    public void ExecuteFixedUpdates()
    {
        currentState.ExecuteFixed();
    }

    public void CheckForValidTransitions()
    {
        StateNode nextState = currentState.CheckStateTransitions();
        if (nextState != null) ChangeState(nextState);
    }
    
    public void PopulateNodeLists()
    {
        stateNodes.Clear();
        transitionNodes.Clear();
        globalTransitions.Clear();
        
        foreach (var node in nodes)
        {
            //If its an StateNode
            var nodeAsState = node as StateNode;
            if (nodeAsState != null)
            {
                stateNodes.Add(nodeAsState);
                continue;
            }
            
            //If its a Transition Node (including global ones)
            var nodeAsTransition = node as TransitionNode;
            if (nodeAsTransition != null && nodeAsTransition)
            {
                transitionNodes.Add(nodeAsTransition);
                continue;
            }
            
            //If its an AnyState node, get transitions
            var nodeAsAnyState = node as AnyStateNode;
            if (nodeAsAnyState != null && nodeAsAnyState)
            {
                NodePort transitionsPort = nodeAsAnyState.GetOutputPort("Transitions");
                transitionsPort.GetConnections().ForEach(c =>
                {
                    globalTransitions.Add(c.node as TransitionNode);
                });
                continue;
            }
            
            //If its an StartNode
            var nodeAsStart = node as StartNode;
            if (nodeAsStart != null)
            {
                startNode = nodeAsStart;
                continue;
            }
        }
    }

    //Init transition nodes with other machine states for dropdown
    public void SendValidStatesToTransitions(List<StateNode> otherStateNodes)
    {
        foreach (var transitionNode in transitionNodes)
        {
            transitionNode.startStateOptions = otherStateNodes;
        }
    }

    //Send other active states to transiton node on request
    public List<StateNode> GetActiveStates()
    {
        return parentMachine.GetActiveStates(this);
    }

    private void EnterStartState()
    {
        currentState = startNode.GetOutputPort("EntryState").Connection.node as StateNode;
        currentState.Enter();
    }

    private void ChangeState(StateNode nextState)
    {
        // Debug.Log($"Change: {currentState.name} -> {nextState.name}");
        currentState.Exit();
        currentState = nextState;
        currentState.Enter();
    }

    private void InitStateNodes()
    {
        foreach (var stateNode in stateNodes)
        {
            stateNode.Initialize(this);
        }
    }

    private void InitTransitionNodes()
    {
        foreach (var transitionNode in transitionNodes)
        {
            transitionNode.Initialize(this);
        }
    }
    

    
}
