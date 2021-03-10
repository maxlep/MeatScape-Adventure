using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class TimerCondition : ITransitionCondition
{
    [SerializeField] [HideLabel] private TimerReference targetParameter;
    [SerializeField] [LabelWidth(175f)] private bool restartOnEnterPreviousState = true;
    
    private string parentTransitionName = "";

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate(List<TriggerVariable> receivedTriggers)
    {
        UpdateTime();
        
        return targetParameter.RemainingTime <= 0f;
    }
    
    public void StartTimer()
    {
        if (restartOnEnterPreviousState)
            targetParameter?.RestartTimer();
    }

    public void UpdateTime()
    {
        targetParameter?.UpdateTime();
    }

    public override string ToString()
    {
        if (targetParameter != null)
            return $"{targetParameter.Name}: {targetParameter.Duration} sec";

        return "<Missing Timer>";
    }
}