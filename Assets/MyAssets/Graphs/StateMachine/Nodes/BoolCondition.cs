using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

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