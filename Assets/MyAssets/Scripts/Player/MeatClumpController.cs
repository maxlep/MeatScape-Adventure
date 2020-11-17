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
    [SerializeField] private FloatReference OrbitRadius;
    [SerializeField] private LayerMask CollisionMask;
    [SerializeField] private LayerMask PlayerCollisionMask;
    [SerializeField] private UnityEvent OnCollideWithStatic;
    [SerializeField] private UnityEvent OnSetMoving;

    public bool ReturningToPlayer {get; private set;}
    public bool OrbitingPlayer {get; private set;}

    private PlayerController playerController;

    private float speed;
    private bool hasCollided = false;
    private bool shouldFallOff = false;
    private bool returnAndOrbit = false;
    private Collider target;
    private LayerMask currentCollisionMask;
    private Vector3 startMovementPoint;

    private float orbitDegrees = 0;

    public void SetPlayerController(PlayerController playerController) {
        this.playerController = playerController;
    }

    private void SetMoving(float speed) {
        if(this.hasCollided) {
            if (shaderUpdater != null) shaderUpdater.ReverseSplat();
            OnSetMoving.Invoke();
        }
        this.hasCollided = false;
        this.shouldFallOff = false;
        this.speed = speed;
        this.currentCollisionMask = CollisionMask;
        this.startMovementPoint = transform.position;
        this.returnAndOrbit = false;
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
        this.OrbitingPlayer = false;
    }

    public void SetReturnToPlayerAndOrbit() {
        this.SetReturnToPlayer();
        this.returnAndOrbit = true;
    }

    private void Update()
    {
        float deltaDistance = this.speed * Time.deltaTime;

        if (!hasCollided) PreMoveCollisionCheck(deltaDistance);
        if (!hasCollided) Move(deltaDistance);
        if (!hasCollided) PostMoveCollisionCheck();
        if(this.OrbitingPlayer) {
            this.OrbitPlayer(deltaDistance);
        }
    }

    private void HandleCollisions(Collider[] colliders, Vector3 normal) {
        if (colliders.Length > 0)
        {
            this.hasCollided = true;
            foreach (var collider in colliders) {
                GameObject hitObj = collider.gameObject;
                
                if(hitObj.layer == layerMapper.GetLayer(LayerEnum.Enemy)) {
                    EnemyController enemyScript = hitObj.GetComponent<EnemyController>();
                    enemyScript.DamageEnemy(1);
                    this.SetReturnToPlayer();
                    return;
                }

                if(hitObj.layer == layerMapper.GetLayer(LayerEnum.Player)) {
                    this.ReturningToPlayer = false;
                    if(this.returnAndOrbit){
                        this.OrbitingPlayer = true;
                        this.orbitDegrees = playerController.ClumpIndex(this) * (360 / playerController.ClumpCount());
                    }
                    if(!this.returnAndOrbit) ReabsorbIntoPlayer();
                    return;
                }
            }
        
            //Static object hit
            if (shaderUpdater != null) shaderUpdater.StartSplat(normal);
            OnCollideWithStatic.Invoke();
        }
    }

    private void PreMoveCollisionCheck(float deltaDistance)
    {
        //SphereCast from current pos to next pos and check for collisions
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, CollisionRadius.Value, transform.forward,
            out hit, deltaDistance, currentCollisionMask, QueryTriggerInteraction.Ignore))
        {
            transform.position += (transform.forward * hit.distance);
            
            this.HandleCollisions(new Collider[]{hit.collider}, hit.normal);
        }
    }

    private void PostMoveCollisionCheck()
    {
        //Overlap sphere at final position to check for intersecting colliders
        Collider[] hitColliders =
            (Physics.OverlapSphere(transform.position, CollisionRadius.Value, currentCollisionMask, QueryTriggerInteraction.Ignore));
        
        this.HandleCollisions(hitColliders, -transform.forward);
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

    private void OrbitPlayer(float deltaDistance) {
        orbitDegrees += deltaDistance;
        if(orbitDegrees > 359) orbitDegrees -= 360;
        float radians = orbitDegrees * Mathf.Deg2Rad;
        Vector2 circlePosition = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * OrbitRadius.Value;
        transform.position = new Vector3(circlePosition.x, 0, circlePosition.y) + playerController.transform.position;
        Vector2 circleDirection = Vector2.Perpendicular(circlePosition).normalized;
        transform.forward = new Vector3(circleDirection.x, transform.forward.y, circleDirection.y);
    }

    private void ReabsorbIntoPlayer()
    {
        playerController.AbsorbClump(this);
        
        if (AbsorbSound != null)
            EffectsManager.Instance?.PlayClipAtPoint(AbsorbSound, transform.position, .6f);
        
        if (AbsorbSystem != null)
            EffectsManager.Instance?.SpawnParticlesAtPoint(AbsorbSystem, transform.position, Quaternion.identity);
    }
}
