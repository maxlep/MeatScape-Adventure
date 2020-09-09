using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class FloatCondition : ITransitionCondition
{
    
    [SerializeField] [HideLabel] private FloatReference targetParameter;
    
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;
    
    private string parentTransitionName = "";

    
    public enum Comparator
    {
        LessThan,
        GreaterThan
    }

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate(TriggerVariable receivedTrigger)
    {
        float paramValue = targetParameter.Value;

        if (comparator == Comparator.GreaterThan)
            return paramValue > value;
        
        if (comparator == Comparator.LessThan)
            return paramValue < value;

        return false;
    }
    
    public override string ToString()
    {
        if (targetParameter != null)
            return $"{targetParameter.Name} {comparator} {value}";
        else
            return "<Missing Float>";
    }
    
}