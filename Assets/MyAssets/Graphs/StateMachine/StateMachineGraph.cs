using System;
using System.Collections.Generic;
using System.Linq;
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
    public List<StateNode> currentStates { get; private set; }
    public LayeredStateMachine parentMachine { get; private set; }

    private List<StartNode> startNodes = new List<StartNode>();
    private VariableContainer parameters;

    #region LifeCycle Methods

    public void ExecuteUpdates()
    {
        CheckForValidTransitions();
        currentStates.ForEach(s => s.Execute());
    }
        
    public void ExecuteFixedUpdates()
    {
        currentStates.ForEach(s => s.ExecuteFixed());
    }

    public void DrawGizmos()
    {
        currentStates.ForEach(s => s.DrawGizmos());
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
        EnterStartStates();
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
        startNodes.Clear();
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
                startNodes.Add(nodeAsStart);
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
        
        //Sort start nodes by execution order
        startNodes =  startNodes.OrderBy(s => s.ExecutionOrderIndex).ToList();
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

    private void EnterStartStates()
    {
        currentStates.Clear();
        
        //Loop through start nodes and enter entry states
        foreach (var startNode in startNodes)
        {
            StateNode entryState = startNode.GetOutputPort("EntryState").Connection.node as StateNode;
            if (entryState != null)
            {
                currentStates.Add(entryState);
                entryState.Enter();
            }
        }
    }
    
    private void CheckForValidTransitions(TriggerVariable receivedTrigger = null)
    {
        List<(StateNode fromState, StateNode toState)> validTransitions = new List<(StateNode fromState, StateNode toState)>();
        
        //Store valid state changes
        for (int i = 0; i < currentStates.Count; i++)
        {
            StateNode nextState = currentStates[i].CheckStateTransitions(receivedTrigger);
            if (nextState != null) validTransitions.Add((currentStates[i], nextState));
        }
        
        foreach (var (exitingState, nextState) in validTransitions)
        {
            ChangeState(exitingState, nextState);
        }
    }
    
    private void ChangeState(StateNode exitingState, StateNode nextState)
    {
        //Debug.Log($"{name}: {exitingState.name} -> {nextState.name}");

        int index = currentStates.IndexOf(exitingState);
        exitingState.Exit();
        currentStates.Insert(index, nextState);
        currentStates.Remove(exitingState);
        nextState.Enter();
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
