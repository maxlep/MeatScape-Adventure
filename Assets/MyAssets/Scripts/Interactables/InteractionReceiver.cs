using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class InteractionReceiver : SerializedMonoBehaviour
{
    [SerializeField] [ListDrawerSettings(DraggableItems = true)] private List<IInteraction> Interactions;

    public void ReceiveInspectInteraction(InspectPayload inspectPayload)
    {
        foreach (var interaction in Interactions)
        {
            if (interaction is InspectInteraction)
                (interaction as InspectInteraction).InvokeInteraction(inspectPayload);
        }
    }
    
    public void ReceiveGroundSlamInteraction(GroundSlamPayload payload)
    {
        foreach (var interaction in Interactions)
        {
            if (interaction is GroundSlamInteraction)
                (interaction as GroundSlamInteraction).InvokeInteraction(payload);
        }
    }
    
    public void ReceiveTriggerEnterInteraction(TriggerEnterPayload triggerEnterPayload)
    {
        foreach (var interaction in Interactions)
        {
            if (interaction is TriggerEnterInteraction)
                (interaction as TriggerEnterInteraction).InvokeInteraction(triggerEnterPayload);
        }
    }
    
    public void ReceiveTriggerExitInteraction(TriggerExitPayload triggerExitPayload)
    {
        foreach (var interaction in Interactions)
        {
            if (interaction is TriggerExitInteraction)
                (interaction as TriggerExitInteraction).InvokeInteraction(triggerExitPayload);
        }
    }
    
    public void ReceiveRollIntoInteraction(RollIntoPayload rollIntoPayload)
    {
        foreach (var interaction in Interactions)
        {
            if (interaction is RollIntoInteraction)
                (interaction as RollIntoInteraction).InvokeInteraction(rollIntoPayload);
        }
    }
    
    public void ReceiveMeateorStirkeIntoInteraction(MeateorStrikeIntoPayload meateorStrikeIntoPayload)
    {
        foreach (var interaction in Interactions)
        {
            if (interaction is MeateorStrikeIntoInteraction)
                (interaction as MeateorStrikeIntoInteraction).InvokeInteraction(meateorStrikeIntoPayload);
        }
    }
    
    public void ReceiveMeatClumpHitInteraction(MeatClumpHitPayload meatClumpHitPayload)
    {
        foreach (var interaction in Interactions)
        {
            if (interaction is MeatClumpHitInteraction)
                (interaction as MeatClumpHitInteraction).InvokeInteraction(meatClumpHitPayload);
        }
    }
    
    public void ReceiveJumpOnInteraction(JumpOnPayload jumpOnPayload)
    {
        foreach (var interaction in Interactions)
        {
            if (interaction is JumpOnInteraction)
                (interaction as JumpOnInteraction).InvokeInteraction(jumpOnPayload);
        }
    }
}