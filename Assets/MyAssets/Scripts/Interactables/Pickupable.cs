using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pickupable : MonoBehaviour
{
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupSystem;
    [SerializeField] private float timeToReachPlayer = .1f;
    [SerializeField] private float targetYOffset = 2f;
    [SerializeField] private UnityEvent OnPickUp;

    private bool movingTowardsPlayer = false;

    private void OnTriggerEnter(Collider other)
    {
        if (movingTowardsPlayer) return;
        
        GameObject otherObj = other.gameObject;
        if (otherObj.layer == LayerMask.NameToLayer("Player"))
        {
            movingTowardsPlayer = true;

            Vector3 targetPos = otherObj.transform.position + Vector3.up * targetYOffset;
            LeanTween.value(gameObject, transform.position, targetPos, timeToReachPlayer)
                .setOnUpdate( (Vector3 p) =>
                {
                    transform.position = p;
                })
                .setOnComplete(PickUp);
        }
    }

    private void PickUp()
    {
        OnPickUp.Invoke();
            
        if (pickupSound != null) 
            EffectsManager.Instance?.PlayClipAtPoint(pickupSound, transform.position, .6f);
            
        if (pickupSystem != null)
            EffectsManager.Instance?.SpawnParticlesAtPoint(pickupSystem, transform.position, Quaternion.identity);
        
        Destroy(this.gameObject);
    }
}
