using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
[HideReferenceObjectPicker]
public class BoolCondition : ITransitionCondition
{
    [SerializeField] [HideLabel] [HideReferenceObjectPicker] 
    private BoolReference targetParameter;
    
    [LabelWidth(40f)] [SerializeField] [HideReferenceObjectPicker]
    private BoolReference value;

    private string parentTransitionName = "";

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate(List<TriggerVariable> receivedTriggers)
    {

        return targetParameter.Value == value.Value;
    }
    
    public override string ToString()
    {
        if (targetParameter != null)
            return value.Value ? $"{targetParameter.Name}" : $"!{targetParameter.Name}";
        else
            return "<Missing Bool>";
    }
    
}