using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class InteractionReceiver : SerializedMonoBehaviour
{
    [SerializeField] [ListDrawerSettings(DraggableItems = true)] private List<IInteraction> Interactions;

    #region Receive Interactions

    public virtual bool ReceiveInspectInteraction(InspectPayload inspectPayload)
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
    
    public virtual bool ReceiveGroundSlamInteraction(GroundSlamPayload payload)
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
    
    public virtual bool ReceiveTriggerEnterInteraction(TriggerEnterPayload triggerEnterPayload)
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
    
    public virtual bool ReceiveTriggerExitInteraction(TriggerExitPayload triggerExitPayload)
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
    
    public virtual bool ReceiveRollIntoInteraction(RollIntoPayload payload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is RollIntoInteraction)
            {
                (interaction as RollIntoInteraction).InvokeInteraction(payload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public virtual bool ReceiveMeateorStirkeIntoInteraction(MeateorStrikeIntoPayload payload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is MeateorStrikeIntoInteraction)
            {
                (interaction as MeateorStrikeIntoInteraction).InvokeInteraction(payload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public virtual bool ReceiveMeatClumpHitInteraction(MeatClumpHitPayload payload)
    {
        bool hasInteraction = false;
        foreach (var interaction in Interactions)
        {
            if (interaction is MeatClumpHitInteraction)
            {
                (interaction as MeatClumpHitInteraction).InvokeInteraction(payload);
                hasInteraction = true;
            }
        }

        return hasInteraction;
    }
    
    public virtual bool ReceiveJumpOnInteraction(JumpOnPayload jumpOnPayload)
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

    #endregion

    #region Respond Interactions

    public virtual void RespondInspectInteraction(InspectPayload inspectPayload) {}

    public virtual void RespondGroundSlamInteraction(GroundSlamPayload payload) {}

    public virtual void RespondTriggerEnterInteraction(TriggerEnterPayload triggerEnterPayload) {}

    public virtual void RespondTriggerExitInteraction(TriggerExitPayload triggerExitPayload) {}
    
    public virtual void RespondRollIntoInteraction(RollIntoPayload payload) {}
    
    public virtual void RespondMeateorStirkeIntoInteraction(MeateorStrikeIntoPayload payload) {}
    
    public virtual void RespondMeatClumpHitInteraction(MeatClumpHitPayload payload) {}

    public virtual void RespondJumpOnInteraction(JumpOnPayload jumpOnPayload) {}

    #endregion
}