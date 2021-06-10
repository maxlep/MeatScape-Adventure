using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class TriggerCondition : ITransitionCondition
{
    [SerializeField] [HideLabel] private TriggerVariable targetParameter;
    
    [Tooltip("Keep this trigger condition on after first match")]
    [LabelWidth(100f)] [InfoBox("Trigger will keep evaluating true after first receive!",
        InfoMessageType.Warning, "KeepTriggerOn")]
    public bool KeepTriggerOn = false;

    private string parentTransitionName = "";
    private bool stayingActive = false;

    public TriggerVariable TargetParameter => targetParameter;


    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    //Check if the trigger variable that was activated matches the one for this condition
    public bool Evaluate(List<TriggerVariable> receivedTriggers)
    {
        if (stayingActive) return true;

        bool triggerMatches = (receivedTriggers != null) ?
            receivedTriggers.Contains(targetParameter) :
            false;
        if (triggerMatches && KeepTriggerOn)
            stayingActive = true;
        
        return triggerMatches;
    }

    public void ResetTriggers()
    {
        stayingActive = false;
    }

    public override string ToString()
    {
        if (targetParameter != null)
            return $"{targetParameter.Name}";
        
        return "<Missing Trigger>";
    }
    
}