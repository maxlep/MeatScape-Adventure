using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;


[Title("MeatClumpHit")]
public class MeatClumpHitInteraction : InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker]
    private UnityEvent<MeatClumpHitPayload> onInteract;

    public void InvokeInteraction(MeatClumpHitPayload meatClumpHitPayload)
    {
        if (onInteract != null && EvaluateConditionals()) onInteract.Invoke(meatClumpHitPayload);
    }
}

public struct MeatClumpHitPayload
{

}