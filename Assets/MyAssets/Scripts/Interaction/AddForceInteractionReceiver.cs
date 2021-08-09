using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddForceInteractionReceiver : InteractionReceiver
{
    [SerializeField] private List<Rigidbody> launchRbs;
    [SerializeField] private bool isImpulse;
    [SerializeField] private float launchForce = 1000f;
    [SerializeField] private bool applyExplosiveForce;
    [SerializeField] private float explosiveForce = 1000f;
    [SerializeField] private float explosiveRadius = 20f;
    
    public override void RespondGroundSlamInteraction(GroundSlamPayload payload)
    {
        base.ReceiveGroundSlamInteraction(payload);
        Launch(payload.hitDir, payload.origin);
    }

    public override void RespondRollIntoInteraction(RollIntoPayload payload)
    {
        base.ReceiveRollIntoInteraction(payload);
        Launch(payload.hitDir, payload.origin);
    }

    public override void RespondMeateorStirkeIntoInteraction(MeateorStrikeIntoPayload payload)
    {
        base.ReceiveMeateorStirkeIntoInteraction(payload);
        Launch(payload.hitDir, payload.origin);
    }

    public override void RespondMeatClumpHitInteraction(MeatClumpHitPayload payload)
    {
        base.ReceiveMeatClumpHitInteraction(payload);
        Launch(payload.hitDir, payload.origin);
    }

    private void Launch(Vector3 dir, Vector3 origin)
    {
        ForceMode forceMode = (isImpulse) ? ForceMode.Impulse : ForceMode.Force;
        
        foreach (var rb in launchRbs)
        {
            rb.AddForce(launchForce * dir, forceMode);
            if (applyExplosiveForce)
                rb.AddExplosionForce(explosiveForce, origin, explosiveRadius);
        }
        
    }
}
