using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class IntCondition
{
    
    [ValueDropdown("GetInts")] [Required]
    [HideLabel] public string TargetParameterName;
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, IntVariable> parameterDict = new Dictionary<string, IntVariable>();

    private string parentTransitionName = "";

    
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
    
    public void Init(VariableContainer machineParameters, string transitionName)
    {
        parameters = machineParameters;
        parentTransitionName = transitionName;
        parameterDict.Clear();
        foreach (var intParam in parameters.GetIntVariables())
        {
            parameterDict.Add(intParam.name, intParam);
        }
    }

    public bool Evaluate()
    {
        if (!parameterDict.ContainsKey(TargetParameterName))
            Debug.LogError($"Transition {parentTransitionName} Int Condition can't find targetParam " + 
                           $"{TargetParameterName}! Did the name of SO parameter change but not update in dropdown?");
        
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