using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;

public class StateNode : Node
{
    [Input]  [PropertyOrder(-3)]  public StateNode previousState;
    [Output] [PropertyOrder(-2)]  public TransitionNode transitions;

    [SerializeField] private string Name;

    protected StateMachineGraph stateMachineGraph;
    protected VariableContainer parameters;
    protected List<TransitionNode> transitionNodes = new List<TransitionNode>();
    protected List<StateReferenceNode> referenceNodes = new List<StateReferenceNode>();
    protected StateNode noTransitionState;

    [SerializeField] [HideInInspector] protected bool zoom = false;
    [HideInInspector] public bool isActiveState = false;
    
    public List<StateNode> previousStates { get; private set; } = new List<StateNode>();
    public List<StateNode> nextStates { get; private set; } = new List<StateNode>();

    public string GetName() => Name;
    public StateMachineGraph GetParentGraph() => stateMachineGraph;

    public bool Zoom
    {
        get => zoom;
        set => zoom = value;
    }

    public void SetParameters(VariableContainer newParams) => parameters = newParams;
    
    #region Init/Dep Injection

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        isActiveState = false;
        PopulateTransitionNodeList();
        PopulateLinkedRefNodes();
        PopulatePreviousStates();
        PopulateNextStates();
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
            
            StateReferenceNode nodeAsRef = (c.node as StateReferenceNode);
            if (nodeAsRef != null && nodeAsRef.ReferencedState != null)
                noTransitionState = nodeAsRef.ReferencedState;
        });
    }

    private void PopulateLinkedRefNodes()
    {
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
        
        foreach (var refNode in referenceNodes)
        {
            previousStates = previousStates.Union(refNode.previousStates).ToList();
        }
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
        
        foreach (var refNode in referenceNodes)
        {
            nextStates = nextStates.Union(refNode.nextStates).ToList();
        }
    }
    
    

    private void OnValidate()
    {
        name = $"{Name} <{this.GetType().Name}>";
    }
    
    #endregion
    
    #region LifeCycle Methods

    public virtual void Enter()
    {
        isActiveState = true;
        foreach (var transition in transitionNodes)
        {
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
    
    public virtual void DrawGizmos()
    {
        
    }
    
    #endregion

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
        
        //Check transitions on reference nodes
        foreach (var refNode in referenceNodes)
        {
            StateNode nextState = refNode.CheckStateTransitions(receivedTrigger);
            if (nextState != null)
            {
                return nextState;
            }
        }

        return null;
    }
    
    [HorizontalGroup("split", 20f)] [PropertyOrder(-1)]
    [Button(ButtonSizes.Small, ButtonStyle.CompactBox, Name = "$GetZoomButtonName")]
    public void ToggleZoom()
    {
        zoom = !zoom;
    }

    private string GetZoomButtonName()
    {
        return zoom ? "+" : "-";
    }
    
    public override string ToString()
    {
        return this.name;
    }
}