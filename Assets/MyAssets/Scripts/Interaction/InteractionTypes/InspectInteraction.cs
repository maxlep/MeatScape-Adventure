using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Title("Inspect")]
public class InspectInteraction : InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker]
    private UnityEvent<InspectPayload> onInteract;
    
    public void InvokeInteraction(InspectPayload inspectPayload)
    {
        if (onInteract != null && EvaluateConditionals()) onInteract.Invoke(inspectPayload);
    }
}

public struct InspectPayload
{
   
}