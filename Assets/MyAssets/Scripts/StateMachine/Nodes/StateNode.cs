using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;

public class StateNode : CollapsableNode
{
    [Input(typeConstraint = TypeConstraint.Strict)]  [PropertyOrder(-3)]  public StateMachineConnection previousState;
    [Output(typeConstraint = TypeConstraint.Strict)] [PropertyOrder(-2)]  public StateMachineConnection transitions;

    [Tooltip("If enabled, this state will be completely skipped if on enter, there are valid transition out of state.")]
    [SerializeField] private bool BypassState = false;

    [SerializeField] protected string Name;
    
    [SerializeField] [TextArea(3, 3)] [PropertySpace(0f, 10f)] 
    [FoldoutGroup("Description", false)] [HideLabel]
    private string Description;

    protected const int LABEL_WIDTH = 175;
        
    protected StateMachineGraph stateMachineGraph;
    protected List<VariableContainer> parameterList;
    protected List<StateReferenceNode> referenceNodes = new List<StateReferenceNode>();
    protected bool isInitialized = false;

    
    [HideInInspector] public bool isActiveState = false;

    public List<StateNode> previousStates { get; private set; } = new List<StateNode>();
    public List<StateNode> nextStates { get; private set; } = new List<StateNode>();
    public List<TransitionNode> previousTransitionNodes { get; private set; } = new List<TransitionNode>();
    public List<TransitionNode> nextTransitionNodes { get; private set; } = new List<TransitionNode>();
    public StateNode nextNoTransitionState { get; private set; }
    public List<StateNode> previousNoTransitionStates { get; private set; } = new List<StateNode>();
    public bool isEntryState { get; private set; }
    public int StartNodeIndex { get; private set; }

    public string GetName() => Name;
    public bool GetBypassState() => BypassState;
    public StateMachineGraph GetParentGraph() => stateMachineGraph;

    public bool IsInitialized
    {
        get => isInitialized;
        set => isInitialized = value;
    }

    public void SetParameters(List<VariableContainer> newParams) => parameterList = newParams;
    
    #region Init/Dep Injection

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        isActiveState = false;
        isInitialized = true;
        PopulateLinkedRefNodes();
        PopulateTransitionNodeLists();
        PopulatePreviousStates();
        PopulateNextStates();
    }

    public virtual void RuntimeInitialize(int startNodeIndex)
    {
        StartNodeIndex = startNodeIndex;
    }

    private void PopulateLinkedRefNodes()
    {
        referenceNodes.Clear();
        linkedNodes.Clear();
        foreach (var stateRefNode in stateMachineGraph.StateReferenceNodes)
        {
            if (stateRefNode.ReferencedState == this)
            {
                referenceNodes.Add(stateRefNode);
                linkedNodes.Add(stateRefNode);
            }
        }
    }

    private void PopulateTransitionNodeLists()
    {
        previousTransitionNodes.Clear();
        nextTransitionNodes.Clear();
        previousNoTransitionStates.Clear();
        nextNoTransitionState = null;
        isEntryState = false;

        //Check this node for transitions (previous and next)
        StoreTransitionsFromNode(this, true);
        StoreTransitionsFromNode(this, false);
        
        //Check ref nodes for transitions (previous and next)
        foreach (var refNode in referenceNodes)
        {
            StoreTransitionsFromNode(refNode, true);
            StoreTransitionsFromNode(refNode, false);
        }
    }

    private void StoreTransitionsFromNode(Node node, bool isInputPort)
    {
        NodePort transitionsPort = (isInputPort) ? node.GetInputPort("previousState")
            : node.GetOutputPort("transitions");
        
        transitionsPort.GetConnections().ForEach(c =>
        {
            //Check for connected transitions
            TransitionNode nodeAsTransition = (c.node as TransitionNode);
            if (nodeAsTransition != null)
            {
                if (isInputPort)
                    previousTransitionNodes.Add(c.node as TransitionNode);
                else
                    nextTransitionNodes.Add(c.node as TransitionNode);
            }
                
            
            //Check for direct state connections
            StateNode nodeAsState = (c.node as StateNode);
            if (nodeAsState != null)
            {
                if (isInputPort)
                {
                    previousNoTransitionStates.Add(nodeAsState);
                }
                else
                {
                    if (nextNoTransitionState != null) 
                        Debug.LogError("More than 1 directly connected state output" +
                                       $"found for node {node.name}! Make sure <2 states" +
                                       $"are connected directly");
                
                    nextNoTransitionState = nodeAsState;
                }
            }
            
            //Check for direct ref node connections
            StateReferenceNode nodeAsRef = (c.node as StateReferenceNode);
            if (nodeAsRef != null && nodeAsRef.ReferencedState != null)
            {
                if (isInputPort)
                {
                    previousNoTransitionStates.Add(nodeAsRef.ReferencedState);
                }
                else
                {
                    if (nextNoTransitionState != null) 
                        Debug.LogError("More than 1 directly connected state output" +
                                       $"found for node {node.name}! Make sure <2 states" +
                                       $"are connected directly");
                
                    nextNoTransitionState = nodeAsRef.ReferencedState;
                }
                
            }
            
            //Check if connected to entry node
            StartNode nodeAsStart = (c.node as StartNode);
            if (nodeAsStart != null)
                isEntryState = true;
        });

        nextTransitionNodes.Sort((node1, node2) => {
            if(node1.TransitionPriority < node2.TransitionPriority) return -1;
            if(node1.TransitionPriority > node2.TransitionPriority) return 1;
            return 0;
        });
    }

    private void PopulatePreviousStates()
    {
        previousStates.Clear();
        
        //Get previous state from direct connection
        foreach (var previousNoTransitionState in previousNoTransitionStates)
        {
            previousStates.Add(previousNoTransitionState);
        }

        //Get previous states from transitions (this node and ref nodes accounted for)
        foreach (var transitionNode in previousTransitionNodes)
        {
            StateNode transState = transitionNode.GetStartingState();
            if (transState != null)
                previousStates.Add(transState);
        }
    }

    private void PopulateNextStates()
    {
        nextStates.Clear();
        
        //Get next state from direct connection
        if (nextNoTransitionState != null)
            nextStates.Add(nextNoTransitionState);

        //Get next states from transitions (this node and ref nodes accounted for)
        foreach (var transitionNode in nextTransitionNodes)
        {
            StateNode transState = transitionNode.GetNextState();
            if (transState != null)
                nextStates.Add(transState);
        }
    }

    
    protected virtual void OnValidate()
    {
        string graphName = (graph != null) ? graph.name : "";
        name = $"{Name} <{this.GetType().Name}> ({graphName})";
    }
    
    #endregion
    
    #region LifeCycle Methods

    public virtual void Awaken()
    {
        
    }

    public virtual void Enter()
    {
        isActiveState = true;
        foreach (var transition in nextTransitionNodes)
        {
            transition.ResetTriggers();
            transition.ResetGameEvents();
            transition.StartTimers();
        }
    }

    public virtual void Execute()
    {
        
    }

    public virtual void ExecuteFixed()
    {
    }

    public virtual void Exit()
    {
        isActiveState = false;
    }

    public virtual void OnApplictionExit()
    {
    }
    
    public virtual void DrawGizmos()
    {
        
    }
    
    #endregion

    public virtual (StateNode, TransitionNode) CheckStateTransitions(List<TriggerVariable> receivedTriggers = null)
    {
        //Check for direct connection to state that bypasses transitions
        if (nextNoTransitionState != null) return (nextNoTransitionState, null);
        
        //Check global transitions
        foreach (var transition in stateMachineGraph.globalTransitions)
        {
            if (transition.EvaluateConditions(receivedTriggers))
            {
                return (transition.GetNextState(), transition);
            }
        }
        
        //Check connected and ref node transitions
        foreach (var transition in nextTransitionNodes)
        {
            if (transition.EvaluateConditions(receivedTriggers))
            {
                return (transition.GetNextState(), transition);
            }
        }

        return (null, null);
    }

    public override string ToString()
    {
        return this.name;
    }
}