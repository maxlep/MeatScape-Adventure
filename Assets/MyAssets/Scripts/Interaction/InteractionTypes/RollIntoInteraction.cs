using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;


[Title("RollInto")]
public class RollIntoInteraction : InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker]
    private UnityEvent<RollIntoPayload> onInteract;

    public void InvokeInteraction(RollIntoPayload rollIntoPayload)
    {
        if (onInteract != null && EvaluateConditionals()) onInteract.Invoke(rollIntoPayload);
    }
}

public struct RollIntoPayload
{
    public Vector3 origin;
    public Vector3 hitDir;
}
