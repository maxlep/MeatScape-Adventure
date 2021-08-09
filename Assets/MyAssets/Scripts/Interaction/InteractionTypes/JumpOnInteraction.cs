using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;


[Title("JumpOn")]
public class JumpOnInteraction : InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker]
    private UnityEvent<JumpOnPayload> onInteract;

    public void InvokeInteraction(JumpOnPayload jumpOnPayload)
    {
        if (onInteract != null && EvaluateConditionals()) onInteract.Invoke(jumpOnPayload);
    }
}

public struct JumpOnPayload
{

}
