using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using XNode;

public class TransitionNode : Node
{
    [Input] public StateNode startingState;
    [Output] public StateNode nextState;

    [Tooltip("Transition only valid if ANY 1 or more of these states are active in OTHER state machine")]
    [ValueDropdown("GetStartStateDropdown")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.88f, 1f, .95f)]  [FoldoutGroup("")]
    public List<StateNode> ValidStartStates;
    
    [Tooltip("Transition only valid if ALL of these Bool condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [FoldoutGroup("")]
    public List<BoolCondition> BoolConditions;
    
    [Tooltip("Transition only valid if ALL of these Trigger condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [FoldoutGroup("")]
    public List<TriggerCondition> TriggerConditions;
    
    [Tooltip("Transition only valid if ALL of these Float condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [FoldoutGroup("")]
    public List<FloatCondition> FloatConditions;
    
    [Tooltip("Transition only valid if ALL of these Int condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [FoldoutGroup("")]
    public List<IntCondition> IntConditions;

    [HideInInspector] public List<StateNode> startStateOptions = new List<StateNode>();
    
    private StateMachineParameters parameters;
    private StateMachineGraph stateMachineGraph;
    private string startingStateName;
    private string nextStateName;

    public void SetParameters(StateMachineParameters newParams) => parameters = newParams;

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        InitNodeName();
        SendParametersToConditions();
    }

    private void InitNodeName()
    {
        var inputConnection = GetInputPort("startingState").Connection;
        var outputConnection = GetOutputPort("nextState").Connection;

        if (inputConnection == null || outputConnection == null)
            return;
        
        StateNode inputState = inputConnection.node as StateNode;
        StateNode outputState = outputConnection.node as StateNode;
        startingStateName = inputState != null ? inputState.Name : "Any State";
        nextStateName = outputState != null ? outputState.Name : "<Missing>";
        name = $"{startingStateName} ---> {nextStateName}";
    }

    public List<StateNode> GetStartStateDropdown()
    {
        return startStateOptions;
    }

    private void OnValidate()
    {
        name = $"{startingStateName} ---> {nextStateName}";
        SendParametersToConditions();

    }

    private void SendParametersToConditions()
    {
        foreach (var boolCondition in BoolConditions)
        {
            boolCondition.parameters = parameters;
        }
        
        foreach (var triggerCondition in TriggerConditions)
        {
            triggerCondition.parameters = parameters;
        }
        
        foreach (var floatCondition in FloatConditions)
        {
            floatCondition.parameters = parameters;
        }
        
        foreach (var intCondition in IntConditions)
        {
            intCondition.parameters = parameters;
        }
    }
    
    

    //Return true if all conditions are met
    public bool EvaluateConditions()
    {
        bool result = true;

        //If no conditions, done
        if (BoolConditions.IsNullOrEmpty() && TriggerConditions.IsNullOrEmpty() &&
            FloatConditions.IsNullOrEmpty() && IntConditions.IsNullOrEmpty() &&
            ValidStartStates.IsNullOrEmpty()) 
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
                result &= triggerCondition.Evaluate();
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

        return result;
    }
}

[System.Serializable]
public class IntCondition
{
    
    [ValueDropdown("GetInts")] [Required]
    [HideLabel] public IntVariable TargetParameter;
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    public enum Comparator
    {
        LessThan,
        GreaterThan,
        EqualTo,
        NotEqualTo
    }
    
    
    private List<IntVariable> GetInts()
    {
        return parameters.IntParameters;
    }

    public bool Evaluate()
    {
        int paramValue = TargetParameter.Value;
        
        if (comparator == Comparator.GreaterThan)
            return paramValue > value;
        
        if (comparator == Comparator.LessThan)
            return paramValue < value;
        
        if (comparator == Comparator.EqualTo)
            return paramValue == value;
        
        if (comparator == Comparator.NotEqualTo)
            return paramValue != value;

        return false;
    }
    
}

[System.Serializable]
public class FloatCondition
{
    
    [ValueDropdown("GetFloats")] [Required]
    [HideLabel] public FloatVariable TargetParameter;
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    public enum Comparator
    {
        LessThan,
        GreaterThan
    }
    
    
    private List<FloatVariable> GetFloats()
    {
        return parameters.FloatParameters;
    }

    public bool Evaluate()
    {
        float paramValue = TargetParameter.Value;

        if (comparator == Comparator.GreaterThan)
            return paramValue > value;
        
        if (comparator == Comparator.LessThan)
            return paramValue < value;

        return false;
    }
    
}


[System.Serializable]
public class BoolCondition
{
    
    [ValueDropdown("GetBools", AppendNextDrawer = true)] [Required]
    [HideLabel] [HideInInlineEditors] public BoolVariable TargetParameter;
    [LabelWidth(40f)] public bool value;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    private List<BoolVariable> GetBools()
    {
        return parameters.BoolParameters;
    }

    public bool Evaluate()
    {
        return TargetParameter.Value == value;
    }
    
}

[System.Serializable]
public class TriggerCondition
{
    
    [ValueDropdown("GetTriggers")] [Required]
    [HideLabel] public BoolVariable TargetParameter;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    private List<BoolVariable> GetTriggers()
    {
        return parameters.TriggerParameters;
    }

    public bool Evaluate()
    {
        return TargetParameter.Value;
    }
    
}
