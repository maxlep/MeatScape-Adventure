using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Title("TriggerExit")]
public class TriggerExitInteraction :  InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker]
    private UnityEvent<TriggerExitPayload> onInteract;

    public void InvokeInteraction(TriggerExitPayload triggerExitPayload)
    {
        if (onInteract != null && EvaluateConditionals()) onInteract.Invoke(triggerExitPayload);
    }
}

public struct TriggerExitPayload
{
    
}