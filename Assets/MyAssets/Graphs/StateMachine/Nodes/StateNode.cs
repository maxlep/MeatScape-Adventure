using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using XNode;

public class StateNode : Node
{
    [Input] public StateNode previousState;
    [Output] public TransitionNode transitions;

    protected StateMachineGraph stateMachineGraph;
    protected StateMachineParameters parameters;
    protected List<TransitionNode> transitionNodes = new List<TransitionNode>();
    
    [ReadOnly] public bool isActiveState = false;
    public string Name;
    
    public void SetParameters(StateMachineParameters newParams) => parameters = newParams;

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
            transitionNodes.Add(c.node as TransitionNode);
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
    
    public override string ToString()
    {
        return this.name;
    }
}