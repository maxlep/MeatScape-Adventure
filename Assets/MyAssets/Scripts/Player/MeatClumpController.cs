using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.ShaderHelpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class MeatClumpController : MonoBehaviour
{
    [SerializeField, Required("Shader Update not set! Splat effect won't trigger!", InfoMessageType.Warning)]
    private MeatClumpShaderUpdater shaderUpdater;
    
    [SerializeField, Required("No Absorb Sound Set!", InfoMessageType.Warning)]
    private AudioClip AbsorbSound;
    
    [SerializeField, Required("No Absorb Particles Set!", InfoMessageType.Warning)]
    private GameObject AbsorbSystem;
    
    [SerializeField] private LayerMapper layerMapper;
    [SerializeField] private FloatReference ClumpReturnSpeed;
    [SerializeField] private FloatReference ClumpReturnAbsorbDistance;
    [SerializeField] private FloatReference CollisionRadius;
    [SerializeField] private LayerMask CollisionMask;
    [SerializeField] private LayerEnum EnemyLayer;
    [SerializeField] private UnityEvent OnCollideWithStatic;

    private PlayerController playerController;
    private float speed;
    private bool hasCollided = false;
    private bool returningToPlayer = false;
    
    public void Initialize(PlayerController playerController, float speed)
    {
        this.playerController = playerController;
        this.speed = speed;
    }

    private void Update()
    {
        float deltaDistance = speed * Time.deltaTime;
        
        if (!hasCollided) HandleCollisions(deltaDistance);
        Move(deltaDistance);
    }

    private void HandleCollisions(float deltaDistance)
    {
        //SphereCast from current pos to next pos and check for collisions
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, CollisionRadius.Value, transform.forward,
            out hit, deltaDistance, CollisionMask))
        {
            hasCollided = true;
            transform.position += transform.forward * hit.distance;
            
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.layer == layerMapper.GetLayer(EnemyLayer))
            {
                EnemyController enemyScript = hitObj.GetComponent<EnemyController>();
                enemyScript.DamageEnemy(1);
                returningToPlayer = true;
            }
            else
            {
                if (shaderUpdater != null) shaderUpdater.StartSplat(hit);
                OnCollideWithStatic.Invoke();
            }
        }
    }

    private void Move(float deltaDistance)
    {
        if (!hasCollided)
        {
            transform.position += transform.forward * deltaDistance;
        }
        else if (returningToPlayer)
        {
            Vector3 targetPos = playerController.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 
                ClumpReturnSpeed.Value * Time.deltaTime);

            //Check if close enough for player to absorb
            float distanceSqrToPlayer = (targetPos - transform.position).sqrMagnitude;
            if (distanceSqrToPlayer < ClumpReturnAbsorbDistance.Value)
            {
                OnAbsorb();
            }
        }
    }

    private void OnAbsorb()
    {
        playerController.CurrentSize += 1;
        
        if (AbsorbSound != null)
            EffectsManager.Instance?.PlayClipAtPoint(AbsorbSound, transform.position, .6f);
        
        if (AbsorbSystem != null)
            EffectsManager.Instance?.SpawnParticlesAtPoint(AbsorbSystem, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }
}
