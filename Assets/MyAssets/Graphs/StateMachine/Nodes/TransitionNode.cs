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
    }

    private void InitNodeName()
    {
        StateNode inputState = (GetInputPort("startingState").Connection.node as StateNode);
        StateNode outputState = (GetOutputPort("nextState").Connection.node as StateNode);
        startingStateName = inputState != null ? inputState.Name : "Any State";
        nextStateName = outputState != null ? outputState.Name : "<Missing>";
        name = $"{startingStateName} ---> {nextStateName}";
        SendParametersToConditions();
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
    
    [ValueDropdown("GetIntNames")] [Required]
    [HideLabel] public string TargetParameter;
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
    
    
    private List<string> GetIntNames()
    {
        return parameters.IntParameters.Keys.ToList();
    }

    public bool Evaluate()
    {
        int paramValue = parameters.GetInt(TargetParameter);
        
        if (comparator == Comparator.GreaterThan)
            return paramValue > value;
        
        else if (comparator == Comparator.LessThan)
            return paramValue < value;
        
        else if (comparator == Comparator.EqualTo)
            return paramValue == value;
        
        else if (comparator == Comparator.NotEqualTo)
            return paramValue != value;

        return false;
    }
    
}

[System.Serializable]
public class FloatCondition
{
    
    [ValueDropdown("GetFloatNames")] [Required]
    [HideLabel] public string TargetParameter;
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    public enum Comparator
    {
        LessThan,
        GreaterThan
    }
    
    
    private List<string> GetFloatNames()
    {
        return parameters.FloatParameters.Keys.ToList();
    }

    public bool Evaluate()
    {
        float paramValue = parameters.GetFloat(TargetParameter);

        if (comparator == Comparator.GreaterThan)
            return paramValue > value;
        
        else if (comparator == Comparator.LessThan)
            return paramValue < value;

        return false;
    }
    
}


[System.Serializable]
public class BoolCondition
{
    
    [ValueDropdown("GetBoolNames")] [Required]
    [HideLabel] public string TargetParameter;
    [HideLabel] public bool value;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    private List<string> GetBoolNames()
    {
        return parameters.BoolParameters.Keys.ToList();
    }

    public bool Evaluate()
    {
        bool paramValue = parameters.GetBool(TargetParameter);
        return paramValue == value;
    }
    
}

[System.Serializable]
public class TriggerCondition
{
    
    [ValueDropdown("GetTriggerNames")] [Required]
    [HideLabel] public string TargetParameter;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    private List<string> GetTriggerNames()
    {
        List<String> triggerNames = new List<string>();
        foreach (var triggerParam in parameters.TriggerParameters)
        {
            return parameters.TriggerParameters.Keys.ToList();
        }

        return triggerNames;
    }

    public bool Evaluate()
    {
        bool paramValue = parameters.GetTrigger(TargetParameter);
        return paramValue;
    }
    
}
