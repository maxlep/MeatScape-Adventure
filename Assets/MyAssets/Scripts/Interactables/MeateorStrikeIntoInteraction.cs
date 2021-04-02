using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Title("MeateorStrikeInto")]
public class MeateorStrikeIntoInteraction : InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker]
    private UnityEvent<MeateorStrikeIntoPayload> onInteract;

    public void InvokeInteraction(MeateorStrikeIntoPayload meateorStrikeIntoPayload)
    {
        if (onInteract != null && EvaluateConditionals()) onInteract.Invoke(meateorStrikeIntoPayload);
    }
}

public struct MeateorStrikeIntoPayload
{

}