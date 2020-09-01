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
        PopulateLinkedRefNodes();
        PopulateTransitionNodeList();
        PopulatePreviousStates();
        PopulateNextStates();
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

    private void PopulateTransitionNodeList()
    {
        transitionNodes.Clear();
        noTransitionState = null;

        //Check this node for transitions
        StoreTransitionsFromNode(this);
        
        //Check ref nodes for transitions
        foreach (var refNode in referenceNodes)
        {
            StoreTransitionsFromNode(refNode);
        }
    }

    private void StoreTransitionsFromNode(Node node)
    {
        NodePort refTransitionsPort = node.GetOutputPort("transitions");
        refTransitionsPort.GetConnections().ForEach(c =>
        {
            //Check for connected transitions
            TransitionNode nodeAsTransition = (c.node as TransitionNode);
            if (nodeAsTransition != null)
                transitionNodes.Add(c.node as TransitionNode);
            
            //Check for direct state connections
            StateNode nodeAsState = (c.node as StateNode);
            if (nodeAsState != null)
            {
                if (noTransitionState != null) 
                    Debug.LogError("More than 1 directly connected state" +
                                   $"found for node {node.name}! Make sure <2 states" +
                                   $"are connected directly");
                
                noTransitionState = nodeAsState;
            }
            
            //Check for direct ref node connections
            StateReferenceNode nodeAsRef = (c.node as StateReferenceNode);
            if (nodeAsRef != null && nodeAsRef.ReferencedState != null)
            {
                if (noTransitionState != null) 
                    Debug.LogError("More than 1 directly connected state" +
                                   $"found for node {node.name}! Make sure <2 states" +
                                   $"are connected directly");
                
                noTransitionState = nodeAsRef.ReferencedState;
            }
                
        });
    }

    private void PopulatePreviousStates()
    {
        previousStates.Clear();

        //Get previous states for this node
        PopulatePreviousStatesForNode(this);
        
        //Get previous states for each ref node
        foreach (var refNode in referenceNodes)
        {
            PopulatePreviousStatesForNode(refNode);
        }
    }

    private void PopulatePreviousStatesForNode(Node node)
    {
        NodePort transitionsPort = node.GetInputPort("previousState");
        transitionsPort.GetConnections().ForEach(c =>
        {
            //Get previous states from transition nodes on input
            TransitionNode nodeAsTransition = (c.node as TransitionNode);
            if (nodeAsTransition != null)
            {
                previousStates.Add(nodeAsTransition.GetStartingState());
            }

            //Get directly connected previous states
            StateNode nodeAsState = (c.node as StateNode);
            if (nodeAsState != null)
                previousStates.Add(nodeAsState);
            
            //Get directly connected previous ref states
            StateReferenceNode nodeAsRef = (c.node as StateReferenceNode);
            if (nodeAsRef != null)
                previousStates.Add(nodeAsRef.ReferencedState);
        });
    }
    
    private void PopulateNextStates()
    {
        nextStates.Clear();
        
        //Get next state from direct connection
        if (noTransitionState != null)
            nextStates.Add(noTransitionState);

        //Get next states from transitions (this node and ref nodes accounted for)
        foreach (var transitionNode in transitionNodes)
        {
            StateNode transState = transitionNode.GetNextState();
            if (transState != null)
                nextStates.Add(transState);
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
        //Check for direct connection to state that bypasses transitions
        if (noTransitionState != null) return noTransitionState;
        
        //Check global transitions
        foreach (var transition in stateMachineGraph.globalTransitions)
        {
            if (transition.EvaluateConditions(receivedTrigger))
            {
                return transition.GetNextState();
            }
        }
        
        //Check connected and ref node transitions
        foreach (var transition in transitionNodes)
        {
            if (transition.EvaluateConditions(receivedTrigger))
            {
                return transition.GetNextState();
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