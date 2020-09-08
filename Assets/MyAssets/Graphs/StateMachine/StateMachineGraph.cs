using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.Utilities;
using UnityEngine;
using XNode;

//To represent connections in state machine graph
[Serializable]
public class StateMachineConnection
{

}

[CreateAssetMenu(fileName = "StateMachineGraph", menuName = "Graphs/StateMachineGraph", order = 0)]
public class StateMachineGraph : NodeGraph
{
    public List<StateNode> stateNodes { get; private set; } = new List<StateNode>();
    public List<TransitionNode> transitionNodes { get; private set; } = new List<TransitionNode>();
    public List<TransitionNode> globalTransitions { get; private set; } = new List<TransitionNode>();
    public List<StateReferenceNode> StateReferenceNodes { get; private set; } = new List<StateReferenceNode>();
    public List<StateNode> currentStates { get; private set; } = new List<StateNode>();
    public LayeredStateMachine parentMachine { get; private set; }
    public bool DebugOnStateChange => debugOnStateChange;

    private List<StartNode> startNodes = new List<StartNode>();
    private HashSet<TriggerVariable> triggersFromTransitions = new HashSet<TriggerVariable>();
    private bool debugOnStateChange = false;

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
    public void StartStateMachine(bool isRuntime)
    {
        SetDebugOnStateChange(false);
        //Set all nodes to NOT initialized
        stateNodes.ForEach(s => s.IsInitialized = false);
        transitionNodes.ForEach(t => t.IsInitialized = false);
        StateReferenceNodes.ForEach(r => r.IsInitialized = false);

        if (isRuntime) startNodes.ForEach(InitNodesRecursively);
        else InitNodes();

        SubscribeToTriggers();
        if (isRuntime) EnterStartStates();
    }

    //Init all nodes on graph, not runtime
    private void InitNodes()
    {
        foreach (var stateNode in stateNodes)
        {
            stateNode.Initialize(this);
        }
        foreach (var transitionNode in transitionNodes)
        {
            transitionNode.Initialize(this);
        }
        foreach (var stateReferenceNode in StateReferenceNodes)
        {
            stateReferenceNode.Initialize(this);
        }
    }

    //Init nodes recursively branching out from each start node
    private void InitNodesRecursively(Node nextNode)
    {
        //Init nodes, return if already initialized
        StateNode nodeAsState = nextNode as StateNode;
        if (nodeAsState != null)
        {
            if (nodeAsState.IsInitialized) return;
            nodeAsState.Initialize(this);
            nodeAsState.RuntimeInitialize();
        }

        TransitionNode nodeAsTransition = nextNode as TransitionNode;
        if (nodeAsTransition != null)
        {
            if (nodeAsTransition.IsInitialized) return;
            nodeAsTransition.Initialize(this);
            nodeAsTransition.RuntimeInitialize();
        }
        
        StateReferenceNode nodeAsReference = nextNode as StateReferenceNode;
        if (nodeAsReference != null)
        {
            if (nodeAsReference.IsInitialized) return;
            nodeAsReference.Initialize(this);
            nodeAsReference.RuntimeInitialize();
        }
        
        //Call recursively for each linked node
        nextNode.LinkedNodes.ForEach(n => InitNodesRecursively(n));

        //Call recursively for each connected node, return if none connected
        nextNode.Outputs.ForEach(output =>
        {
            var connections = output.GetConnections();
            if (connections.Count < 1) return;
            
            connections.ForEach(c => InitNodesRecursively(c.node));
        });
    }

    private void SubscribeToTriggers()
    {
        foreach (var transitionNode in transitionNodes)
        {
            foreach (var transitionTrigger in transitionNode.TriggerVars)
            {
                triggersFromTransitions.Add(transitionTrigger);
            }
        }

        foreach (var triggerVar in triggersFromTransitions)
        {
            triggerVar.OnUpdate += () => CheckForValidTransitions(triggerVar);
        }
    }
    
    public void InjectDependencies(LayeredStateMachine parentMachine)
    {
        this.parentMachine = parentMachine;
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
            transitionNode.SetStartStates(otherStateNodes);
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
        if (debugOnStateChange) Debug.LogError($"{name}: {exitingState.name} -> {nextState.name}");

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

    private void SetDebugOnStateChange(bool enable)
    {
        this.debugOnStateChange = enable;
    }

    public void ToggleDebugOnStateChange()
    {
        debugOnStateChange = !debugOnStateChange;
    }
    

    
}
