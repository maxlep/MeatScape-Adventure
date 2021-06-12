using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
[HideReferenceObjectPicker]
public class FloatCondition : ITransitionCondition
{
    
    [SerializeField] [HideLabel] [HideReferenceObjectPicker] 
    private FloatReference targetParameter;
    
    [HideLabel] public Comparator comparator;
    [HideLabel] public FloatValueReference value;
    
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

    public bool Evaluate(List<TriggerVariable> receivedTriggers)
    {
        float paramValue = targetParameter.Value;

        if (comparator == Comparator.GreaterThan)
            return paramValue > value.Value;
        
        if (comparator == Comparator.LessThan)
            return paramValue < value.Value;

        return false;
    }
    
    public override string ToString()
    {
        if (targetParameter != null)
            return $"{targetParameter.Name} {comparator} {value.Name}";
        else
            return "<Missing Float>";
    }
    
}