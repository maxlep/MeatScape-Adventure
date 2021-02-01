using System;
using System.Collections;
using System.Collections.Generic;
using Den.Tools;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private List<AudioClip> impactSounds;
    [SerializeField] private GameObject impactParticles;
    [SerializeField] private GameObject muzzleParticles;
    [SerializeField] private LayerMask impactMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageRadius = 10f;

    private void Awake()
    {
        Quaternion rot = Quaternion.LookRotation(transform.forward);
        
        if (muzzleParticles != null)
            EffectsManager.Instance.SpawnParticlesAtPoint(muzzleParticles, transform.position, rot);
    }

    private void OnCollisionEnter(Collision other)
    {
        GameObject otherObj = other.gameObject;

        if (otherObj.IsInLayerMask(impactMask))
        {
            if (!impactSounds.IsNullOrEmpty())
                EffectsManager.Instance.PlayClipAtPoint(PickRandomClip(impactSounds), transform.position, .15f);
            if (impactParticles != null)
                EffectsManager.Instance.SpawnParticlesAtPoint(impactParticles, transform.position, quaternion.identity);

            ExplosiveDamage();
            Destroy(gameObject);
        }
        
    }

    private void ExplosiveDamage()
    {
        Collider[] HitColliders = Physics.OverlapSphere(transform.position, damageRadius, playerMask);

        foreach (var collider in HitColliders)
        {
            PlayerController playerScript = collider.GetComponent<PlayerController>();
            if (playerScript != null)
                playerScript.Damage(damage, Vector3.zero, 0f);
        }
    }


    private AudioClip PickRandomClip(List<AudioClip> audioClips)
    {
        int randomIndex = Random.Range(0, impactSounds.Count);
        return audioClips[randomIndex];
    }

    
}
