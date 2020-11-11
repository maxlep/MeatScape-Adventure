using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    [SerializeField] private FloatReference ClumpFalloffDistance;
    [SerializeField] private FloatReference ClumpFalloffSpeed;
    [SerializeField] private FloatReference PlayerReturnTime;
    [SerializeField] private FloatReference PlayerReturnDistanceThreshold;
    [SerializeField] private FloatReference PlayerReturnMaxSpeed;
    [SerializeField] private FloatReference PlayerReturnMinSpeed;
    [SerializeField] private FloatReference CollisionRadius;
    [SerializeField] private LayerMask CollisionMask;
    [SerializeField] private LayerMask PlayerCollisionMask;
    [SerializeField] private UnityEvent OnCollideWithStatic;
    [SerializeField] private UnityEvent OnSetMoving;

    public bool ReturningToPlayer {get; private set;}

    private PlayerController playerController;

    private float speed;
    private bool hasCollided = false;
    private bool shouldFallOff = false;
    private Collider target;
    private LayerMask currentCollisionMask;
    private Vector3 startMovementPoint;

    public void SetPlayerController(PlayerController playerController) {
        this.playerController = playerController;
    }

    private void SetMoving(float speed) {
        this.hasCollided = false;
        this.shouldFallOff = false;
        this.speed = speed;
        this.currentCollisionMask = CollisionMask;
        this.startMovementPoint = transform.position;
        if (shaderUpdater != null) shaderUpdater.ReverseSplat();
        OnSetMoving.Invoke();
    }

    public void SetMoving(float speed, Vector3 direction) {
        this.SetMoving(speed);
        this.transform.forward = direction.normalized;
        this.target = null;
    }

    public void SetMoving(float speed, Collider target) {
        this.SetMoving(speed);
        this.target = target;
    }

    public void SetReturnToPlayer()
    {
        float distance = (playerController.Collider.bounds.center - transform.position).magnitude;
        float speed = distance / PlayerReturnTime.Value;
        if(distance >= PlayerReturnDistanceThreshold.Value) speed = Mathf.Min(speed, PlayerReturnMaxSpeed.Value);
        speed = Mathf.Max(speed, PlayerReturnMinSpeed.Value);
        this.SetMoving(speed, playerController.Collider);
        this.currentCollisionMask = PlayerCollisionMask;
        this.ReturningToPlayer = true;
    }

    private void Update()
    {
        float deltaDistance = this.speed * Time.deltaTime;

        if (!hasCollided) HandleCollisions(deltaDistance);
        if (!hasCollided) Move(deltaDistance);
        if (!hasCollided) PostMoveCollisionCheck();
    }

    private void HandleCollisions(float deltaDistance)
    {
        //SphereCast from current pos to next pos and check for collisions
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, CollisionRadius.Value, transform.forward,
            out hit, deltaDistance, currentCollisionMask, QueryTriggerInteraction.Ignore))
        {   
            this.hasCollided = true;
            transform.position += (transform.forward * hit.distance);
            GameObject hitObj = hit.collider.gameObject;
            
            if(hitObj.layer == layerMapper.GetLayer(LayerEnum.Enemy)) {
                EnemyController enemyScript = hitObj.GetComponent<EnemyController>();
                enemyScript.DamageEnemy(1);
                this.SetReturnToPlayer();
                return;
            }

            if(hitObj.layer == layerMapper.GetLayer(LayerEnum.Player)) {
                ReabsorbIntoPlayer();
                return;
            }
            
            //Static object hit
            if (shaderUpdater != null) shaderUpdater.StartSplat(hit.normal);
            OnCollideWithStatic.Invoke();
        }
    }

    private void PostMoveCollisionCheck()
    {
        //Overlap sphere at final position to check for intersecting colliders
        Collider[] hitColliders =
            (Physics.OverlapSphere(transform.position, CollisionRadius.Value, currentCollisionMask));
        
        if (hitColliders.Length > 0)
        {
            this.hasCollided = true;
            foreach (var collider in hitColliders)
            {
                if (collider.gameObject.layer == layerMapper.GetLayer(LayerEnum.Player))
                {
                    ReabsorbIntoPlayer();
                    return;
                }
            }

            if (shaderUpdater != null) shaderUpdater.StartSplat(-transform.forward);
            OnCollideWithStatic.Invoke();
        }
    }

    private void Move(float deltaDistance)
    {
        shouldFallOff = Vector3.Distance(transform.position, this.startMovementPoint) >= this.ClumpFalloffDistance.Value;
        if (target != null) {
            transform.forward = target.bounds.center - transform.position;
        }
        if(!this.ReturningToPlayer && shouldFallOff) {
            float newY = transform.forward.y - (this.ClumpFalloffSpeed.Value * Time.deltaTime);
            transform.forward = new Vector3(transform.forward.x, newY, transform.forward.z);
        }
        transform.position += transform.forward * deltaDistance;
    }

    private void ReabsorbIntoPlayer()
    {
        this.ReturningToPlayer = false;
        playerController.AbsorbClump(this, transform.forward);

        if (AbsorbSound != null)
            EffectsManager.Instance?.PlayClipAtPoint(AbsorbSound, transform.position, .6f);
        
        if (AbsorbSystem != null)
            EffectsManager.Instance?.SpawnParticlesAtPoint(AbsorbSystem, transform.position, Quaternion.identity);
    }
}
