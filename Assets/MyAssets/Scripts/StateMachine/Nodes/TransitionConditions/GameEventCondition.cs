using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameEventCondition : ITransitionCondition
{
    [SerializeField] [HideLabel] private GameEvent targetParameter;
    
    [Tooltip("Keep this game event condition on after first match")]
    [LabelWidth(100f)] [InfoBox("Game Event will keep evaluating true after first receive!",
        InfoMessageType.Warning, "KeepGameEventOn")]
    public bool KeepGameEventOn = false;

    private string parentTransitionName = "";
    private bool stayingActive = false;

    private bool eventReceived;

    public GameEvent  TargetParameter => targetParameter;


    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
        targetParameter.Subscribe(ReceiveGameEvent);
    }

    //Check if the trigger variable that was activated matches the one for this condition
    public bool Evaluate(List<TriggerVariable> receivedTriggers)
    {
        if (stayingActive) return true;
        
        if (eventReceived)
        {
            eventReceived = false;
            return true;
        }

        return false;
    }

    public void ResetGameEvent()
    {
        stayingActive = false;
        eventReceived = false;
    }

    private void ReceiveGameEvent()
    {
        eventReceived = true;
        if (KeepGameEventOn) stayingActive = true;
    }

    public override string ToString()
    {
        if (targetParameter != null)
            return $"{targetParameter.name}";
        
        return "<Missing GameEvent>";
    }
}
