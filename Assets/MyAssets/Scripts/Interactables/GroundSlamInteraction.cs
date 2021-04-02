using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Title("GroundSlam")]
public class GroundSlamInteraction : InteractionBase
{
    [SerializeField] [HideReferenceObjectPicker] 
    private UnityEvent<GroundSlamPayload> onInteract;
    
    
    public void InvokeInteraction(GroundSlamPayload groundSlamPayload)
    {
        if (onInteract != null && EvaluateConditionals()) onInteract.Invoke(groundSlamPayload);
    }
}

public struct GroundSlamPayload
{
    public Vector3 origin;
}