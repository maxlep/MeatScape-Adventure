using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class AddForceInteractionReceiver : InteractionReceiver
{
    [SerializeField] private bool horizontalOnly;
    [SerializeField] private bool isImpulse;
    [SerializeField] private float launchForce = 1000f;
    [SerializeField] private bool applyExplosiveForce;
    [SerializeField] private float explosiveForce = 1000f;
    [SerializeField] private float explosiveRadius = 20f;
    [SerializeField] private float cooldown = .05f;
    [SerializeField] private List<Rigidbody> launchRbs = new List<Rigidbody>();

    private float lastActivateTime = Mathf.NegativeInfinity;

    [Button("Populate Rigidbodies")]
    public void PopulateRigidbodies()
    {
        GetChildRigidbodies(transform);
    }

    private void GetChildRigidbodies(Transform targetTransform)
    {
        foreach(Transform child in targetTransform)
        {
            Rigidbody childRb = child.gameObject.GetComponent<Rigidbody>();
            if (childRb != null) launchRbs.Add(childRb);
            GetChildRigidbodies(child);
        }
    }
    
    public override void RespondGroundSlamInteraction(GroundSlamPayload payload)
    {
        base.RespondGroundSlamInteraction(payload);
        Launch(payload.hitDir, payload.origin);
    }

    public override void RespondRollIntoInteraction(RollIntoPayload payload)
    {
        base.RespondRollIntoInteraction(payload);
        Launch(payload.hitDir, payload.origin);
        
    }

    public override void RespondMeateorStirkeIntoInteraction(MeateorStrikeIntoPayload payload)
    {
        base.RespondMeateorStirkeIntoInteraction(payload);
        Launch(payload.hitDir, payload.origin);
        Debug.Log("Respond Meateor Strike Into");
    }

    public override void RespondMeatClumpHitInteraction(MeatClumpHitPayload payload)
    {
        base.RespondMeatClumpHitInteraction(payload);
        Launch(payload.hitDir, payload.origin);
        Debug.Log("Respond Clump Hit");
    }

    private void Launch(Vector3 dir, Vector3 origin)
    {
        if (lastActivateTime + cooldown > Time.time)
            return;
        
        Debug.Log("Launch");

        ForceMode forceMode = (isImpulse) ? ForceMode.Impulse : ForceMode.Force;
        Vector3 forceDir = dir;
        if (horizontalOnly) forceDir = forceDir.xoz(); 
        
        foreach (var rb in launchRbs)
        {
            rb.AddForce(launchForce * forceDir, forceMode);
            if (applyExplosiveForce)
                rb.AddExplosionForce(explosiveForce, origin, explosiveRadius);
        }

        lastActivateTime = Time.time;

    }
}
