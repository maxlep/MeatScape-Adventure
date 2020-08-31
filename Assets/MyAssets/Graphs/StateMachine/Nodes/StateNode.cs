using System;
using System.Collections;
using System.Collections.Generic;
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
    protected StateNode noTransitionState;

    [SerializeField] [HideInInspector] private float Zoom = .5f;
    [HideInInspector] public bool isActiveState = false;

    public string GetName() => Name;

    public float GetZoom() => Zoom;

    public void SetParameters(VariableContainer newParams) => parameters = newParams;

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        isActiveState = false;
        PopulateTransitionNodeList();
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

    protected override void Init()
    {
        base.Init();
    }

    private void OnValidate()
    {
        name = $"{Name} <{this.GetType().Name}>";
    }

    public virtual void Enter()
    {
        isActiveState = true;
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

    public virtual StateNode CheckStateTransitions(TriggerVariable receivedTrigger = null)
    {
        //Check for direct connection to state that bypasses transtions
        if (noTransitionState != null) return noTransitionState;
        
        //Check global transitions
        foreach (var transition in stateMachineGraph.globalTransitions)
        {
            if (transition.EvaluateConditions(receivedTrigger))
            {
                NodePort nextStatePort = transition.GetOutputPort("nextState");
                return nextStatePort.Connection.node as StateNode;
            }
        }
        
        //Check connected transitions
        foreach (var transition in transitionNodes)
        {
            if (transition.EvaluateConditions(receivedTrigger))
            {
                NodePort nextStatePort = transition.GetOutputPort("nextState");
                return nextStatePort.Connection.node as StateNode;
            }
        }

        return null;
    }
    
    [HorizontalGroup("split", 20f)] [PropertyOrder(-1)]
    [Button(ButtonSizes.Small, ButtonStyle.CompactBox, Name = "-")]
    public void DecreaseZoom()
    {
        if (Zoom > .5f) Zoom = .5f;
        else Zoom = 0f;
    }

    [HorizontalGroup("split/right", 20f)] [PropertyOrder(0)]
    [Button(ButtonSizes.Small, ButtonStyle.CompactBox, Name = "+")] 
    public void IncreaseZoom()
    {
        if (Zoom < .5f) Zoom = .5f;
        else Zoom = 1f;
    }
    
    public override string ToString()
    {
        return this.name;
    }
}