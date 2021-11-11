using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;


[Title("MeatClumpHit")]
public class MeatClumpHitInteraction : InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker]
    private UnityEvent<MeatClumpHitPayload> onInteract;

    [SerializeField] [HideReferenceObjectPicker]
    private Transform clumpStickObj;

    public void InvokeInteraction(MeatClumpHitPayload meatClumpHitPayload)
    {
        if (onInteract != null && EvaluateConditionals())
        {
            var transform = !clumpStickObj.SafeIsUnityNull() ? clumpStickObj : meatClumpHitPayload.hitObj;
            meatClumpHitPayload.clumpObj.SetParent(transform);
            
            onInteract.Invoke(meatClumpHitPayload);
        }
    }
}

public struct MeatClumpHitPayload
{
    public Vector3 origin;
    public Vector3 hitDir;
    public Transform hitObj;
    public Transform clumpObj;
}