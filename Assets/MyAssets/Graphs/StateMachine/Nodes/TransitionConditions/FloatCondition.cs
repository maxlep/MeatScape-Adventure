﻿using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class FloatCondition
{
    
    [ValueDropdown("GetFloats")] [Required]
    [HideLabel] public string TargetParameterName;
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, FloatVariable> parameterDict = new Dictionary<string, FloatVariable>();

    private string parentTransitionName = "";

    
    public enum Comparator
    {
        LessThan,
        GreaterThan
    }
    
    
    private List<string> GetFloats()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }
    
    public void Init(VariableContainer machineParameters, string transitionName)
    {
        parameters = machineParameters;
        parentTransitionName = transitionName;
        parameterDict.Clear();
        foreach (var floatParam in parameters.GetFloatVariables())
        {
            parameterDict.Add(floatParam.name, floatParam);
        }
    }

    public bool Evaluate()
    {
        if (!parameterDict.ContainsKey(TargetParameterName))
            Debug.LogError($"Transition {parentTransitionName} Float Condition can't find targetParam " + 
                           $"{TargetParameterName}! Did the name of SO parameter change but not update in dropdown?");
        
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