using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using XNode;

public class TransitionNode : Node
{
    [Input] [PropertyOrder(-3)] public StateNode startingState;
    [Output] [PropertyOrder(-2)] public StateNode nextState;

    [TextArea] [HideLabel]
    [SerializeField] private string ConditionPreview;

    [Tooltip("Transition only valid if ANY 1 or more of these states are active in OTHER state machine")]
    [ValueDropdown("GetStartStateDropdown")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.88f, 1f, .95f)]  [FoldoutGroup("", Expanded = true)]
    [SerializeField] private List<StateNode> ValidStartStates;

    [Tooltip("Transition only valid if ALL of these Bool condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [FoldoutGroup("")]
    [SerializeField] private List<BoolCondition> BoolConditions;

    [Tooltip("Transition only valid if ALL of these Trigger condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [FoldoutGroup("")]
    [SerializeField] private List<TriggerCondition> TriggerConditions;

    [Tooltip("Transition only valid if ALL of these Float condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [FoldoutGroup("")]
    [SerializeField] private List<FloatCondition> FloatConditions;

    [Tooltip("Transition only valid if ALL of these Int condition are met")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [FoldoutGroup("")]
    [SerializeField] private List<IntCondition> IntConditions;


    [SerializeField] [HideInInspector] private float Zoom = .5f;
    [HideInInspector] public List<StateNode> startStateOptions = new List<StateNode>();
    
    private VariableContainer parameters;
    private StateMachineGraph stateMachineGraph;
    private string startingStateName;
    private string nextStateName;

    public void SetParameters(VariableContainer newParams) => parameters = newParams;
    public float GetZoom() => Zoom;
    

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        InitNodeName();
        InitConditions();
    }

    private void InitNodeName()
    {
        var inputConnection = GetInputPort("startingState").Connection;
        var outputConnection = GetOutputPort("nextState").Connection;

        if (inputConnection == null || outputConnection == null)
            return;
        
        StateNode inputState = inputConnection.node as StateNode;
        StateNode outputState = outputConnection.node as StateNode;
        startingStateName = inputState != null ? inputState.GetName() : "Any State";
        nextStateName = outputState != null ? outputState.GetName() : "<Missing>";
        name = $"{startingStateName} -> {nextStateName}";
    }

    public List<StateNode> GetStartStateDropdown()
    {
        return startStateOptions;
    }

    private void OnValidate()
    {
        name = $"{startingStateName} -> {nextStateName}";
        InitConditions();
        
    }

    private void InitConditions()
    {
        ConditionPreview = "";
        
        foreach (var startState in ValidStartStates)
        {
            ConditionPreview += $"Start: {startState}\n";
        }
        foreach (var boolCondition in BoolConditions)
        {
            boolCondition.Init(parameters);
            ConditionPreview += $"{boolCondition}\n";
        }
        
        foreach (var triggerCondition in TriggerConditions)
        {
            triggerCondition.Init(parameters);
            ConditionPreview += $"{triggerCondition}\n";
        }
        
        foreach (var floatCondition in FloatConditions)
        {
            floatCondition.Init(parameters);
            ConditionPreview += $"{floatCondition}\n";
        }
        
        foreach (var intCondition in IntConditions)
        {
            intCondition.Init(parameters);
            ConditionPreview += $"{intCondition}\n";
        }
    }


    //Return true if all conditions are met
    //Optional trigger can be sent
    public bool EvaluateConditions(TriggerVariable receivedTrigger = null)
    {
        bool result = true;
        
        //If triggers present but none passed, will never be true
        if (!TriggerConditions.IsNullOrEmpty() && receivedTrigger == null)
            return false;

        
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

        return result;
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
}

[System.Serializable]
public class IntCondition
{
    
    [ValueDropdown("GetInts")] [Required]
    [HideLabel] public string TargetParameterName;
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, IntVariable> parameterDict = new Dictionary<string, IntVariable>();

    
    public enum Comparator
    {
        LessThan,
        GreaterThan,
        EqualTo,
        NotEqualTo
    }
    
    
    private List<string> GetInts()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }
    
    public void Init(VariableContainer machineParameters)
    {
        parameters = machineParameters;
        parameterDict.Clear();
        foreach (var intParam in parameters.GetIntVariables())
        {
            parameterDict.Add(intParam.name, intParam);
        }
    }

    public bool Evaluate()
    {
        int paramValue = parameterDict[TargetParameterName].Value;
        
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

    public override string ToString()
    {
        return $"{TargetParameterName} {comparator} {value}";
    }
}

[System.Serializable]
public class FloatCondition
{
    
    [ValueDropdown("GetFloats")] [Required]
    [HideLabel] public string TargetParameterName;
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, FloatVariable> parameterDict = new Dictionary<string, FloatVariable>();

    
    public enum Comparator
    {
        LessThan,
        GreaterThan
    }
    
    
    private List<string> GetFloats()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }
    
    public void Init(VariableContainer machineParameters)
    {
        parameters = machineParameters;
        parameterDict.Clear();
        foreach (var floatParam in parameters.GetFloatVariables())
        {
            parameterDict.Add(floatParam.name, floatParam);
        }
    }

    public bool Evaluate()
    {
        float paramValue = parameterDict[TargetParameterName].Value;

        if (comparator == Comparator.GreaterThan)
            return paramValue > value;
        
        if (comparator == Comparator.LessThan)
            return paramValue < value;

        return false;
    }
    
    public override string ToString()
    {
        return $"{TargetParameterName} {comparator} {value}";
    }
    
}


[System.Serializable]
public class BoolCondition
{
    
    [ValueDropdown("GetBoolNames")] [Required]
    [HideLabel] public string TargetParameterName;
    [LabelWidth(40f)] public bool value;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, BoolVariable> parameterDict = new Dictionary<string, BoolVariable>();
    
    private List<String> GetBoolNames()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }

    public void Init(VariableContainer machineParameters)
    {
        parameters = machineParameters;
        parameterDict.Clear();
        foreach (var boolParam in parameters.GetBoolVariables())
        {
            parameterDict.Add(boolParam.name, boolParam);
        }
    }

    public bool Evaluate()
    {
        return parameterDict[TargetParameterName].Value == value;
    }
    
    public override string ToString()
    {
        return value ? $"{TargetParameterName}" : $"!{TargetParameterName}";
    }
    
}

[System.Serializable]
public class TriggerCondition
{
    
    [ValueDropdown("GetTriggerNames")] [Required]
    [HideLabel] public string TargetParameterName;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, TriggerVariable> parameterDict = new Dictionary<string, TriggerVariable>();
    public TriggerVariable GetTriggerVariable() => parameterDict[TargetParameterName];

    
    private List<String> GetTriggerNames()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }
    
    public void Init(VariableContainer machineParameters)
    {
        parameters = machineParameters;
        parameterDict.Clear();
        foreach (var triggerParam in parameters.GetTriggerVariables())
        {
            parameterDict.Add(triggerParam.name, triggerParam);
        }
    }

    //Check if the trigger variable that was activated matches the one for this condition
    public bool Evaluate(TriggerVariable ReceivedTrigger)
    {
        return parameterDict[TargetParameterName].Equals(ReceivedTrigger);
    }
    
    public override string ToString()
    {
        return $"{TargetParameterName}";
    }
    
}
