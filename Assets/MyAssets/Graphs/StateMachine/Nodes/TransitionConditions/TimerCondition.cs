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

    private string parentTransitionName = "";

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate(TriggerVariable receivedTrigger)
    {
        UpdateTime();
        
        return targetParameter.RemainingTime <= 0f;
    }
    
    public void StartTimer()
    {
        targetParameter?.StartTimer();
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