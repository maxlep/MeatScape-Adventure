using System;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;
using XNode;

[CreateAssetMenu(fileName = "StateMachineGraph", menuName = "Graphs/StateMachineGraph", order = 0)]
public class StateMachineGraph : NodeGraph
{
    public List<StateNode> stateNodes { get; private set; } = new List<StateNode>();
    public List<TransitionNode> transitionNodes { get; private set; } = new List<TransitionNode>();
    public List<TransitionNode> globalTransitions { get; private set; } = new List<TransitionNode>();
    public List<StateReferenceNode> StateReferenceNodes { get; private set; } = new List<StateReferenceNode>();
    public StateNode currentState { get; private set; }
    public LayeredStateMachine parentMachine { get; private set; }

    private StartNode startNode;
    private VariableContainer parameters;

    #region LifeCycle Methods

    public void ExecuteUpdates()
    {
        CheckForValidTransitions();
        currentState.Execute();
    }
        
    public void ExecuteFixedUpdates()
    {
        currentState.ExecuteFixed();
    }

    #endregion

    #region Init/Dep Injection

    //For call by layered state machine to begin
    public void StartStateMachine()
    {
        InitStateNodes();
        InitTransitionNodes();
        InitStateReferenceNodes();
        SubscribeToTriggers();
        EnterStartState();
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

    private void InitStateReferenceNodes()
    {
        foreach (var stateRefNode in StateReferenceNodes)
        {
            stateRefNode.Initialize(this);
        }
    }

    private void SubscribeToTriggers()
    {
        foreach (var triggerVar in parameters.GetTriggerVariables())
        {
            triggerVar.OnUpdate += () => CheckForValidTransitions(triggerVar);
        }
    }
    
    public void InjectDependencies(LayeredStateMachine parentMachine, VariableContainer parameters)
    {
        this.parentMachine = parentMachine;
        this.parameters = parameters;
    }
    
     public void PopulateNodeLists()
    {
        stateNodes.Clear();
        transitionNodes.Clear();
        globalTransitions.Clear();
        StateReferenceNodes.Clear();
        
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
            
            //If its an StateReferenceNode
            var nodeAsRef = node as StateReferenceNode;
            if (nodeAsRef != null)
            {
                StateReferenceNodes.Add(nodeAsRef);
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

    #endregion

    #region State Management

    private void EnterStartState()
    {
        currentState = startNode.GetOutputPort("EntryState").Connection.node as StateNode;
        currentState.Enter();
    }
    
    private void CheckForValidTransitions(TriggerVariable receivedTrigger = null)
    {
        StateNode nextState = currentState.CheckStateTransitions(receivedTrigger);
        if (nextState != null) ChangeState(nextState);
    }
    
    private void ChangeState(StateNode nextState)
    {
        //Debug.Log($"Change: {currentState.name} -> {nextState.name}");
        currentState.Exit();
        currentState = nextState;
        currentState.Enter();
    }

    #endregion

    //Send other active states to transiton node on request
    public List<StateNode> GetActiveStates()
    {
        return parentMachine.GetActiveStates(this);
    }

    public void ToggleExpandAll(bool expanded)
    {
        foreach (var transitionNode in transitionNodes)
        {
            transitionNode.Zoom = expanded;
        }
        foreach (var stateNode in stateNodes)
        {
            stateNode.Zoom = expanded;
        }
    }
    

    
}
