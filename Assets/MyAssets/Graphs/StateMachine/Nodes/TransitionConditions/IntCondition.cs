using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class IntCondition
{
    
    [SerializeField] [HideLabel] private IntReference targetParameter;
    
    [HideLabel] public Comparator comparator;
    [HideLabel] public float value;

    private string parentTransitionName = "";

    
    public enum Comparator
    {
        LessThan,
        GreaterThan,
        EqualTo,
        NotEqualTo
    }
    

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate()
    {
        int paramValue = targetParameter.Value;
        
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
        if (targetParameter != null)
            return $"{targetParameter.Name} {comparator} {value}";
        else
            return "<Missing Int>";
    }
}