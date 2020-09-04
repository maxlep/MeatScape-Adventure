﻿using System;
using System.Collections.Generic;
using System.Reflection;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using XNode;

public class TransitionNode : Node
{
    [Input] [PropertyOrder(-3)] public StateNode startingState;
    [Output (connectionType = ConnectionType.Override)] [PropertyOrder(-2)] public StateNode nextState;

    [TextArea(3,10)] [HideLabel]
    [SerializeField] private string conditionPreview;

    [Tooltip("Transition only valid if ANY 1 or more of these states are active in OTHER state machine")]
    [ValueDropdown("GetStartStateDropdown")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.88f, 1f, .95f)]  [HideIf("$zoom")]
    [SerializeField] private List<StateNode> ValidStartStates = new List<StateNode>();

    [Tooltip("Transition only valid if ALL of these Bool condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")]
    [SerializeField] private List<BoolCondition> BoolConditions = new List<BoolCondition>();

    [Tooltip("Transition only valid if ALL of these Trigger condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")]
    [SerializeField] private List<TriggerCondition> TriggerConditions = new List<TriggerCondition>();

    [Tooltip("Transition only valid if ALL of these Float condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")]
    [SerializeField] private List<FloatCondition> FloatConditions = new List<FloatCondition>();

    [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")]
    [SerializeField] private List<IntCondition> IntConditions = new List<IntCondition>();
    
    [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")]
    [SerializeField] private List<TimerCondition> TimerConditions = new List<TimerCondition>();


    private bool zoom = false;
    private List<StateNode> startStateOptions = new List<StateNode>();
    private List<TriggerVariable> triggerVars = new List<TriggerVariable>();

    public List<TriggerVariable> TriggerVars => triggerVars;
    public string ConditionPreview => conditionPreview;


    protected bool isInitialized = false;
    private List<VariableContainer> parameterList;
    private StateMachineGraph stateMachineGraph;
    private string startingStateName;
    private string nextStateName;

    public void SetStartStates(List<StateNode> startStates) => startStateOptions = startStates;
    public bool Zoom
    {
        get => zoom;
        set => zoom = value;
    }
    
    public bool IsInitialized
    {
        get => isInitialized;
        set => isInitialized = value;
    }

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        triggerVars.Clear();
        isInitialized = true;
        InitNodeName();
        InitConditions();
    }
    
    public virtual void RuntimeInitialize()
    {
        
    }

    private void InitNodeName()
    {
        var inputConnection = GetInputPort("startingState").Connection;
        StateNode outputState = GetNextState();

        if (inputConnection == null || outputState == null)
            return;
        
        StateNode inputState = inputConnection.node as StateNode;
        
        startingStateName = inputState != null ? inputState.GetName() : "Any State";
        nextStateName = outputState != null ? outputState.GetName() : "<Missing>";
        name = $"{startingStateName} -> {nextStateName}";
    }

    public List<StateNode> GetStartStateDropdown()
    {
        return startStateOptions;
    }

    public StateNode GetNextState()
    {
        var outputConnection = GetOutputPort("nextState").Connection;

        if (outputConnection == null) return null;
        
        StateNode connectionAsState = outputConnection.node as StateNode;
        StateReferenceNode connectionAsRef = outputConnection.node as StateReferenceNode;

        if (connectionAsState != null)
            return connectionAsState;

        else if (connectionAsRef != null)
            return connectionAsRef.ReferencedState;

        return null;
    }
    
    public StateNode GetStartingState()
    {
        var inputConnection = GetInputPort("startingState").Connection;

        if (inputConnection == null) return null;
        
        StateNode connectionAsState = inputConnection.node as StateNode;
        StateReferenceNode connectionAsRef = inputConnection.node as StateReferenceNode;

        if (connectionAsState != null)
            return connectionAsState;

        else if (connectionAsRef != null)
            return connectionAsRef.ReferencedState;

        return null;
    }

    private void OnValidate()
    {
        InitConditions();
        name = $"{startingStateName} -> {nextStateName}";
    }

    public void StartTimers()
    {
        foreach (var timerCondition in TimerConditions)
        {
            timerCondition.StartTimer();
        }
    }

    private void InitConditions()
    {
        conditionPreview = "";
        
        foreach (var startState in ValidStartStates)
        {
            conditionPreview += $"- Start: {startState.GetName()}\n";
        }
        foreach (var boolCondition in BoolConditions)
        {
            boolCondition.Init(name);
            conditionPreview += $"- {boolCondition}\n";
        }
        
        foreach (var triggerCondition in TriggerConditions)
        {
            triggerCondition.Init(name);
            if (triggerCondition.TargetParameter != null) 
                triggerVars.Add(triggerCondition.TargetParameter);
            conditionPreview += $"- {triggerCondition}\n";
        }
        
        foreach (var floatCondition in FloatConditions)
        {
            floatCondition.Init(name);
            conditionPreview += $"- {floatCondition}\n";
        }
        
        foreach (var intCondition in IntConditions)
        {
            intCondition.Init(name);
            conditionPreview += $"- {intCondition}\n";
        }
        
        foreach (var timerCondition in TimerConditions)
        {
            timerCondition.Init(name);
            conditionPreview += $"- {timerCondition}\n";
        }
    }


    //Return true if all conditions are met
    //Optional trigger can be sent
    public bool EvaluateConditions(TriggerVariable receivedTrigger = null)
    {
        bool result = true;
        
        //If no conditions, done
        if (BoolConditions.IsNullOrEmpty() && TriggerConditions.IsNullOrEmpty() &&
            FloatConditions.IsNullOrEmpty() && IntConditions.IsNullOrEmpty() &&
            ValidStartStates.IsNullOrEmpty() && TimerConditions.IsNullOrEmpty()) 
            return true;
        
        //Check Valid start states (OR)
        if (!ValidStartStates.IsNullOrEmpty())
        {
            bool isValidStateActive = false;
            List<StateNode> activeStates = stateMachineGraph.GetActiveStates();

            ValidStartStates.ForEach(validState => activeStates.ForEach(activeState =>
            {
                if (validState == activeState) isValidStateActive = true;
            }));
            if (!isValidStateActive) return false;
        }

        //Check BoolConditions (AND)
        if (!BoolConditions.IsNullOrEmpty())
        {
            foreach (var boolCondition in BoolConditions)
            {
                result &= boolCondition.Evaluate();
                if (!result) return false;
            }
        }

        //Check TriggerConditions (AND)
        if (!TriggerConditions.IsNullOrEmpty())
        {
            foreach (var triggerCondition in TriggerConditions)
            {
                result &= triggerCondition.Evaluate(receivedTrigger);
                if (!result) return false;
            }
        }

        //Check FloatConditions (AND)
        if (!FloatConditions.IsNullOrEmpty())
        {
            foreach (var floatCondition in FloatConditions)
            {
                result &= floatCondition.Evaluate();
                if (!result) return false;
            }
        }
        
        //Check IntConditions (AND)
        if (!IntConditions.IsNullOrEmpty())
        {
            foreach (var intCondition in IntConditions)
            {
                result &= intCondition.Evaluate();
                if (!result) return false;
            }
        }
        
        //Check TimerConditions (AND)
        if (!TimerConditions.IsNullOrEmpty())
        {
            foreach (var timerCondition in TimerConditions)
            {
                result &= timerCondition.Evaluate();
                if (!result) return false;
            }
        }

        return result;
    }
    

    [HorizontalGroup("split", 30f)] [PropertyOrder(-1)]
    [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Name = "$GetZoomButtonName")]
    public void ToggleZoom()
    {
        zoom = !zoom;
    }

    private string GetZoomButtonName()
    {
        return zoom ? "+" : "-";
    }
}