using System;
using System.Collections.Generic;
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
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.88f, 1f, .95f)] 
    public List<StateNode> ValidStartStates;
    
    [Tooltip("Transition only valid if ALL of these Bool condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
    public List<BoolCondition> BoolConditions;
    
    [Tooltip("Transition only valid if ALL of these Trigger condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
    public List<TriggerCondition> TriggerConditions;
    
    [Tooltip("Transition only valid if ALL of these Float condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
    public List<FloatCondition> FloatConditions;
    
    [Tooltip("Transition only valid if ALL of these Int condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
    public List<IntCondition> IntConditions;

    [HideInInspector] public StateMachineParameters parameters;
    [HideInInspector] public List<StateNode> startStateOptions = new List<StateNode>();
    
    private StateMachineGraph stateMachineGraph;
    private string startingStateName;
    private string nextStateName;

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
    }

    public List<StateNode> GetStartStateDropdown()
    {
        return startStateOptions;
    }

    private void OnValidate()
    {
        name = $"{startingStateName} ---> {nextStateName}";
        
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
    
    [ValueDropdown("GetIntNames")]
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
        List<String> intNames = new List<string>();
        foreach (var intParam in parameters.IntParameters)
        {
            intNames.Add(intParam.name);
        }

        return intNames;
    }

    public bool Evaluate()
    {
        //Find bool in SO and compare value
        foreach (var intParam in parameters.IntParameters)
        {
            if (!intParam.name.Equals(TargetParameter))
                continue;

            if (comparator == Comparator.GreaterThan)
                return intParam.value > value;
            
            else if (comparator == Comparator.LessThan)
                return intParam.value < value;
            
            else if (comparator == Comparator.EqualTo)
                return intParam.value == value;
            
            else if (comparator == Comparator.NotEqualTo)
                return intParam.value != value;

        }

        Debug.LogError($"Parameter {TargetParameter} could not be found in SO! Did you leave a ");
        return false;
    }
    
}

[System.Serializable]
public class FloatCondition
{
    
    [ValueDropdown("GetFloatNames")]
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
        List<String> floatNames = new List<string>();
        foreach (var floatParam in parameters.FloatParameters)
        {
            floatNames.Add(floatParam.name);
        }

        return floatNames;
    }

    public bool Evaluate()
    {
        //Find bool in SO and compare value
        foreach (var floatParam in parameters.FloatParameters)
        {
            if (!floatParam.name.Equals(TargetParameter))
                continue;

            if (comparator == Comparator.GreaterThan)
                return floatParam.value > value;
            
            else if (comparator == Comparator.LessThan)
                return floatParam.value < value;

        }

        Debug.LogError($"Parameter {TargetParameter} could not be found in SO!");
        return false;
    }
    
}


[System.Serializable]
public class BoolCondition
{
    
    [ValueDropdown("GetBoolNames")]
    [HideLabel] public string TargetParameter;
    [HideLabel] public bool value;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    private List<string> GetBoolNames()
    {
        List<String> boolNames = new List<string>();
        foreach (var boolParam in parameters.BoolParameters)
        {
            boolNames.Add(boolParam.name);
        }

        return boolNames;
    }

    public bool Evaluate()
    {
        //Find bool in SO and compare value
        foreach (var boolParam in parameters.BoolParameters)
        {
            if (boolParam.name.Equals(TargetParameter))
            {
                return value == boolParam.value;
            }
        }
        
        Debug.LogError($"Parameter {TargetParameter} could not be found in SO!");
        return false;
    }
    
}

[System.Serializable]
public class TriggerCondition
{
    
    [ValueDropdown("GetTriggerNames")]
    [HideLabel] public string TargetParameter;
    
    [HideInInspector] public StateMachineParameters parameters;
    
    private List<string> GetTriggerNames()
    {
        List<String> triggerNames = new List<string>();
        foreach (var triggerParam in parameters.TriggerParameters)
        {
            triggerNames.Add(triggerParam.name);
        }

        return triggerNames;
    }

    public bool Evaluate()
    {
        //Find bool in SO and compare value
        foreach (var triggerParam in parameters.TriggerParameters)
        {
            if (triggerParam.name.Equals(TargetParameter))
            {
                return triggerParam.value;
            }
        }
        
        Debug.LogError($"Parameter {TargetParameter} could not be found in SO!");
        return false;
    }
    
}
