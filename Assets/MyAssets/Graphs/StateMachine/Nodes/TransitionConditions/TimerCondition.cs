using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class TimerCondition
{
    [ValueDropdown("GetContainerNames")] [Required] [HideIf("$UseConstant")] 
    [HideLabel] public string TargetContainerName;
    
    [ValueDropdown("GetTimerNames")] [Required][ShowIf("HasSelectedContainer")]
    [HideIf("$UseConstant")] 
    [HideLabel]  public string TargetParameterName;
    
    [ShowIf("$UseConstant")]  [SerializeField] [LabelWidth(65f)]
    [LabelText("Duration")] private float ConstantDuration;
    
    [ShowIf("$UseConstant")] [ShowInInspector] [LabelWidth(65f)]
    [Sirenix.OdinInspector.ReadOnly] [LabelText("Remaining")] private float ConstantRemainingTime;
    
    [LabelWidth(80f)] public bool UseConstant;
    
    [HideInInspector] public List<VariableContainer> parameterList;
    [HideInInspector] public Dictionary<string, Dictionary<string, TimerVariable>> parameterDict = 
        new Dictionary<string, Dictionary<string, TimerVariable>>();

    private float startTime = Mathf.NegativeInfinity;
    private string parentTransitionName = "";

    private List<String> GetContainerNames()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }

    private List<String> GetTimerNames()
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
            Dictionary<string, TimerVariable> nameToVarDict = new Dictionary<string, TimerVariable>();
            foreach (var timerParam in p.GetTimerVariables())
            {
                nameToVarDict.Add(timerParam.name, timerParam);
            }
            parameterDict.Add(p.name, nameToVarDict);
        });
    }

    public bool Evaluate()
    {
        UpdateTime();
        
        if (UseConstant)
            return ConstantRemainingTime <= 0f;
        
        if (!parameterDict.ContainsKey(TargetContainerName) || 
            !parameterDict[TargetContainerName].ContainsKey(TargetParameterName))
            Debug.LogError($"Transition {parentTransitionName} Timer Condition can't find targetParam " + 
                           $"{TargetParameterName}! Did the name of SO parameter or its container change but not" +
                           $" update in dropdown?");
        
        return parameterDict[TargetContainerName][TargetParameterName].RemainingTime <= 0f;
    }
    
    public void StartTimer()
    {
        startTime = Time.time;

        if (UseConstant)
            ConstantRemainingTime = ConstantDuration;
        else
            parameterDict[TargetContainerName][TargetParameterName]?.StartTimer();

    }

    public void UpdateTime()
    {
        if (UseConstant)
            ConstantRemainingTime = Mathf.Max(0f, ConstantDuration - (Time.time - startTime));
        else
            parameterDict[TargetContainerName][TargetParameterName]?.UpdateTime();
        
    }

    public override string ToString()
    {
        if (UseConstant)
            return $"<ConstTimer>: {ConstantDuration} sec";
        
        if (TargetParameterName != null)
            return $"{TargetParameterName}: {parameterDict[TargetContainerName][TargetParameterName].Duration} sec";

        return "";
    }
}