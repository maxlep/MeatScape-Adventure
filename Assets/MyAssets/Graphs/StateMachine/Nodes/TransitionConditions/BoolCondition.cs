using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class BoolCondition
{
    [SerializeField] [HideLabel] private BoolReference targetParameter;
    
    [LabelWidth(40f)] [SerializeField] private bool value;

    private string parentTransitionName = "";

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate()
    {

        return targetParameter.Value == value;
    }
    
    public override string ToString()
    {
        if (targetParameter != null)
            return value ? $"{targetParameter.Name}" : $"!{targetParameter.Name}";
        else
            return "<Missing Bool>";
    }
    
}