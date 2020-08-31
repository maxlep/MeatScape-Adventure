using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class TriggerCondition
{
    
    [ValueDropdown("GetTriggerNames")] [Required]
    [HideLabel] public string TargetParameterName;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, TriggerVariable> parameterDict = new Dictionary<string, TriggerVariable>();
    public TriggerVariable GetTriggerVariable() => parameterDict[TargetParameterName];

    private string parentTransitionName = "";

    
    private List<String> GetTriggerNames()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }
    
    public void Init(VariableContainer machineParameters, string transitionName)
    {
        parameters = machineParameters;
        parentTransitionName = transitionName;
        parameterDict.Clear();
        foreach (var triggerParam in parameters.GetTriggerVariables())
        {
            parameterDict.Add(triggerParam.name, triggerParam);
        }
    }

    //Check if the trigger variable that was activated matches the one for this condition
    public bool Evaluate(TriggerVariable ReceivedTrigger)
    {
        if (!parameterDict.ContainsKey(TargetParameterName))
            Debug.LogError($"Transition {parentTransitionName} Timer Condition can't find targetParam " + 
                           $"{TargetParameterName}! Did the name of SO parameter change but not update in dropdown?");
        return parameterDict[TargetParameterName].Equals(ReceivedTrigger);
    }
    
    public override string ToString()
    {
        return $"{TargetParameterName}";
    }
    
}