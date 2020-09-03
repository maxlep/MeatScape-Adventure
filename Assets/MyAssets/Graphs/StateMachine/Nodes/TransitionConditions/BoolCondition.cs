using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class BoolCondition
{
    [ValueDropdown("GetContainerNames")] [Required]
    [HideLabel] public string TargetContainerName;
    
    [ValueDropdown("GetBoolNames")] [Required][ShowIf("HasSelectedContainer")]
    [HideLabel]  public string TargetParameterName;
    
    [LabelWidth(40f)] public bool value;
    
    [HideInInspector] public List<VariableContainer> parameterList;
    [HideInInspector] public Dictionary<string, Dictionary<string, BoolVariable>> parameterDict = 
        new Dictionary<string, Dictionary<string, BoolVariable>>();

    private string parentTransitionName = "";

    private List<String> GetContainerNames()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }

    private List<String> GetBoolNames()
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
            Dictionary<string, BoolVariable> nameToVarDict = new Dictionary<string, BoolVariable>();
            foreach (var boolParam in p.GetBoolVariables())
            {
                nameToVarDict.Add(boolParam.name, boolParam);
            }
            parameterDict.Add(p.name, nameToVarDict);
            
            
        });
        
    }

    public bool Evaluate()
    {
        if (!parameterDict.ContainsKey(TargetContainerName) || 
            !parameterDict[TargetContainerName].ContainsKey(TargetParameterName))
            Debug.LogError($"Transition {parentTransitionName} Bool Condition can't find targetParam {TargetParameterName}!" +
                           $"Did the name of SO parameter or its container change but not update in dropdown?");

        var paramValue = parameterDict[TargetContainerName][TargetParameterName].Value;
        return paramValue == value;
    }
    
    public override string ToString()
    {
        return value ? $"{TargetParameterName}" : $"!{TargetParameterName}";
    }
    
}