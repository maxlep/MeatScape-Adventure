using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Title("TriggerEnter")]
public class TriggerEnterInteraction :  InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker]
    private UnityEvent<TriggerEnterPayload> onInteract;

    public void InvokeInteraction(TriggerEnterPayload triggerEnterPayload)
    {
        if (onInteract != null && EvaluateConditionals()) onInteract.Invoke(triggerEnterPayload);
    }
}

public struct TriggerEnterPayload
{
    
}