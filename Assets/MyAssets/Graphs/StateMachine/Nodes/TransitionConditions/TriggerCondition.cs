using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class TriggerCondition
{
    [ValueDropdown("GetContainerNames")] [Required]
    [HideLabel] public string TargetContainerName;
    
    [ValueDropdown("GetFloatNames")] [Required][ShowIf("HasSelectedContainer")]
    [HideLabel]  public string TargetParameterName;
    
    [HideInInspector] public List<VariableContainer> parameterList;
    [HideInInspector] public Dictionary<string, Dictionary<string, TriggerVariable>> parameterDict = 
        new Dictionary<string, Dictionary<string, TriggerVariable>>();

    private string parentTransitionName = "";

    private List<String> GetContainerNames()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }

    private List<String> GetFloatNames()
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
            Dictionary<string, TriggerVariable> nameToVarDict = new Dictionary<string, TriggerVariable>();
            foreach (var triggerParam in p.GetTriggerVariables())
            {
                nameToVarDict.Add(triggerParam.name, triggerParam);
            }
            parameterDict.Add(p.name, nameToVarDict);
        });
    }

    //Check if the trigger variable that was activated matches the one for this condition
    public bool Evaluate(TriggerVariable ReceivedTrigger)
    {
        if (!parameterDict.ContainsKey(TargetContainerName) || 
            !parameterDict[TargetContainerName].ContainsKey(TargetParameterName))
            Debug.LogError($"Transition {parentTransitionName} Trigger Condition can't find targetParam " + 
                           $"{TargetParameterName}! Did the name of SO parameter or its container change but not update in dropdown?");
        return parameterDict[TargetContainerName][TargetParameterName].Equals(ReceivedTrigger);
    }
    
    public override string ToString()
    {
        return $"{TargetParameterName}";
    }
    
}