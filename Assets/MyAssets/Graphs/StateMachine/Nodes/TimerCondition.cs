using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class TimerCondition
{
    [ValueDropdown("GetTimerNames")] [Required] [SerializeField]
    [HideIf("$UseConstant")] 
    [HideLabel] private string TargetParameterName;
    
    [ShowIf("$UseConstant")]  [SerializeField] [LabelWidth(65f)]
    [LabelText("Duration")] private float ConstantDuration;
    
    [ShowIf("$UseConstant")] [ShowInInspector] [LabelWidth(65f)]
    [LabelText("Remaining")] private float ConstantRemainingTime;
    
    [LabelWidth(80f)] public bool UseConstant;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, TimerVariable> parameterDict = new Dictionary<string, TimerVariable>();

    private float startTime = Mathf.NegativeInfinity;
    
    private List<string> GetTimerNames()
    {
        return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    }

    public void Init(VariableContainer machineParameters)
    {
        parameters = machineParameters;
        parameterDict.Clear();
        foreach (var timerParam in parameters.GetTimerVariables())
        {
            parameterDict.Add(timerParam.name, timerParam);
        }
    }

    public bool Evaluate()
    {
        UpdateTime();
        
        if (UseConstant)
            return ConstantRemainingTime <= 0f;
        
        return parameterDict[TargetParameterName].RemainingTime <= 0f;
    }
    
    public void StartTimer()
    {
        startTime = Time.time;
        parameterDict[TargetParameterName]?.StartTimer();
    }

    public void UpdateTime()
    {
        parameterDict[TargetParameterName]?.UpdateTime();
        ConstantRemainingTime = Mathf.Max(0f, ConstantDuration - (Time.time - startTime));
    }

    public override string ToString()
    {
        if (UseConstant)
            return $"<ConstTimer>: {ConstantDuration} sec";
        
        if (TargetParameterName != null)
            return $"{TargetParameterName}: {parameterDict[TargetParameterName].Duration} sec";

        return "";
    }
}