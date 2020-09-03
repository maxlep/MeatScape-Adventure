using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class IntCondition
{
    
    [ValueDropdown("GetContainerNames")] [Required]
    [HideLabel] public string TargetContainerName;
    
    [ValueDropdown("GetIntNames")] [Required][ShowIf("HasSelectedContainer")]
    [HideLabel]  public string TargetParameterName;
    
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    [HideInInspector] public List<VariableContainer> parameterList;
    [HideInInspector] public Dictionary<string, Dictionary<string, IntVariable>> parameterDict = 
        new Dictionary<string, Dictionary<string, IntVariable>>();

    private string parentTransitionName = "";

    
    public enum Comparator
    {
        LessThan,
        GreaterThan,
        EqualTo,
        NotEqualTo
    }
    
    private List<String> GetContainerNames()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }

    private List<String> GetIntNames()
    {
        //Return bool names if selected container
        if (parameterDict.Count > 0 && HasSelectedContainer() && parameterDict.ContainsKey(TargetContainerName))
            return parameterDict[TargetContainerName].Keys.ToList();

        return new List<string>() {""};
    }

    private bool HasSelectedContainer()
    {
        if (TargetContainerName == null || TargetContainerName.Equals(""))
            return false;

        return true;
    }

    public void Init(List<VariableContainer> machineParameters, string transitionName)
    {
        parameterList = machineParameters;
        parentTransitionName = transitionName;
        parameterDict.Clear();
        
        //Init dictionary that maps VariableContainerName -> VarName -> Var
        parameterList.ForEach(p =>
        {
            Dictionary<string, IntVariable> nameToVarDict = new Dictionary<string, IntVariable>();
            foreach (var intParam in p.GetIntVariables())
            {
                nameToVarDict.Add(intParam.name, intParam);
            }
            parameterDict.Add(p.name, nameToVarDict);
        });
    }

    public bool Evaluate()
    {
        if (!parameterDict.ContainsKey(TargetContainerName) || 
            !parameterDict[TargetContainerName].ContainsKey(TargetParameterName))
            Debug.LogError($"Transition {parentTransitionName} Int Condition can't find targetParam " + 
                           $"{TargetParameterName}! Did the name of SO parameter or its container change but not update in dropdown?");
        
        int paramValue = parameterDict[TargetContainerName][TargetParameterName].Value;
        
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