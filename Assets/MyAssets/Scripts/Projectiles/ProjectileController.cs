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

    private Vector3 currVelocity;
    private Vector3 prevVelocity;
    
    #region Lifecycle
    private void Awake()
    {
        Quaternion rot = Quaternion.LookRotation(transform.forward);
        
        if (muzzleParticles != null)
            EffectsManager.Instance.SpawnParticlesAtPoint(muzzleParticles, transform.position, rot);
    }

    private void FixedUpdate()
    {
        PreMoveCollisionCheck();
        Move();
        PostMoveCollisionCheck();
    }
    #endregion
    
    #region Setters
    public void SetMoving(float speed, Vector3 direction) {
        this.currVelocity = speed * direction.normalized;
    }
    #endregion
    
    #region Collision
    private void HandleCollisions(Collider[] colliders, Vector3 normal) {
        if (colliders.Length > 0)
        {
            foreach (var collider in colliders) {
                GameObject otherObj = collider.gameObject;

                if (otherObj.IsInLayerMask(impactMask))
                {
                    if (!impactSounds.IsNullOrEmpty())
                        EffectsManager.Instance.PlayClipAtPoint(PickRandomClip(impactSounds), transform.position, .10f);
                    if (impactParticles != null)
                        EffectsManager.Instance.SpawnParticlesAtPoint(impactParticles, transform.position, quaternion.identity);

                    ExplosiveDamage();
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }

    private void PreMoveCollisionCheck()
    {
        //SphereCast from current pos to next pos and check for collisions
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward,
            out hit, (currVelocity * Time.deltaTime).magnitude, impactMask, QueryTriggerInteraction.Ignore))
        {
            transform.position += (transform.forward * hit.distance);
            
            this.HandleCollisions(new Collider[]{hit.collider}, hit.normal);
        }
    }

    private void PostMoveCollisionCheck()
    {
        //Overlap sphere at final position to check for intersecting colliders
        Collider[] hitColliders =
            (Physics.OverlapSphere(transform.position, 0.5f, impactMask, QueryTriggerInteraction.Ignore));
        
        this.HandleCollisions(hitColliders, -transform.forward);
    }
    #endregion
    
    #region Movement
    private void Move()
    {
        currVelocity += Physics.gravity * Time.deltaTime;
        transform.position += currVelocity * Time.deltaTime;
        prevVelocity = currVelocity;
    }
    #endregion

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
