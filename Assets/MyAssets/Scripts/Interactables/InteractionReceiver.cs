using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class InteractionReceiver : SerializedMonoBehaviour
{
    [SerializeField] [ListDrawerSettings(DraggableItems = true)] private List<IInteraction> Interactions;

    public bool ReceiveInspectInteraction(InspectPayload inspectPayload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is InspectInteraction)
            {
                (interaction as InspectInteraction).InvokeInteraction(inspectPayload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public bool ReceiveGroundSlamInteraction(GroundSlamPayload payload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is GroundSlamInteraction)
            {
                (interaction as GroundSlamInteraction).InvokeInteraction(payload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public bool ReceiveTriggerEnterInteraction(TriggerEnterPayload triggerEnterPayload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is TriggerEnterInteraction)
            {
                (interaction as TriggerEnterInteraction).InvokeInteraction(triggerEnterPayload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public bool ReceiveTriggerExitInteraction(TriggerExitPayload triggerExitPayload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is TriggerExitInteraction)
            {
                (interaction as TriggerExitInteraction).InvokeInteraction(triggerExitPayload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public bool ReceiveRollIntoInteraction(RollIntoPayload rollIntoPayload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is RollIntoInteraction)
            {
                (interaction as RollIntoInteraction).InvokeInteraction(rollIntoPayload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public bool ReceiveMeateorStirkeIntoInteraction(MeateorStrikeIntoPayload meateorStrikeIntoPayload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is MeateorStrikeIntoInteraction)
            {
                (interaction as MeateorStrikeIntoInteraction).InvokeInteraction(meateorStrikeIntoPayload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public bool ReceiveMeatClumpHitInteraction(MeatClumpHitPayload meatClumpHitPayload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is MeatClumpHitInteraction)
            {
                (interaction as MeatClumpHitInteraction).InvokeInteraction(meatClumpHitPayload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public bool ReceiveJumpOnInteraction(JumpOnPayload jumpOnPayload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is JumpOnInteraction)
            {
                (interaction as JumpOnInteraction).InvokeInteraction(jumpOnPayload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
}