using MoreMountains.Feedbacks;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.ShaderHelpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using MyAssets.Scripts.Utils;
using Sirenix.Utilities;

public class MeatClumpController : MonoBehaviour
{
    [SerializeField, Required("Shader Update not set! Splat effect won't trigger!", InfoMessageType.Warning)]
    private MeatClumpShaderUpdater shaderUpdater;
    
    [SerializeField, Required("No Absorb Sound Set!", InfoMessageType.Warning)]
    private AudioClip AbsorbSound;
    
    [SerializeField, Required("No Absorb Particles Set!", InfoMessageType.Warning)]
    private GameObject AbsorbSystem;
    
    [SerializeField] private Transform meshTransform;
    [SerializeField] private IntReference ClumpDamage;
    [SerializeField] private IntReference ClumpOverchargedDamage;
    [SerializeField] private FloatReference ClumpScalingFactor;
    [SerializeField] private FloatReference ClumpGravityEnableTime;
    [SerializeField] private FloatReference ClumpDragEnableTime;
    [SerializeField] private FloatReference ClumpGravityFactor;
    [SerializeField] private FloatReference ClumpDestroyTime;
    [SerializeField] private FloatReference DragCoefficient;
    [SerializeField] private FloatReference DragThresholdVelocity;
    [SerializeField] private LayerMask CollisionMaskEnemy;
    [SerializeField] private LayerMask CollisionMaskInteractable;
    [SerializeField] private UnityEvent OnCollideWithStatic;
    [SerializeField] private UnityEvent OnCollideWithEnemy;
    [SerializeField] private IntVariable enemyHitId;
    [SerializeField] private MMFeedbacks impactFeedbacks;
    [SerializeField] private MMFeedbacks overChargeFeedbacks;
    [SerializeField] protected DynamicGameEvent SpawnMeatEvent;
    [SerializeField] private bool DropMeatOnParentEnemyDeath = true;

    private float startTime;
    private float damage;
    private Vector3 currentVelocity;
    private bool hasCollided;
    private bool hasGravity;
    private bool hasDrag;

    #region Lifecycle
    private void Awake() {
        transform.localScale *= ClumpScalingFactor.Value;
        damage = ClumpDamage.Value;
    }

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if(!hasCollided)
        {
            //Enable Drag after clump has been moving for ClumpDragEnableTime duration.
            if(!hasDrag) hasDrag = Time.time - startTime >= ClumpDragEnableTime.Value;
            if (hasDrag)
            {
                Vector3 horizontalDrag = Mathf.Max(0f,(currentVelocity.sqrMagnitude - DragThresholdVelocity.Value)) 
                                         * -currentVelocity.normalized * DragCoefficient.Value * Time.deltaTime;
                currentVelocity += horizontalDrag;
            }

            //Enable gravity after clump has been moving for ClumpGravityEnableTime duration.
            if(!hasGravity) hasGravity = Time.time - startTime >= ClumpGravityEnableTime.Value;
            if(hasGravity) currentVelocity += Physics.gravity * ClumpGravityFactor.Value * Time.deltaTime;
            Vector3 deltaDistance = currentVelocity * Time.deltaTime;

            Move(deltaDistance);
        }
    }
    #endregion
    
    #region Setters
    public void SetMoving(float speed, Vector3 direction) {
        currentVelocity = speed * direction.normalized;
        meshTransform.forward = direction.normalized;     //Point mesh in direction of velocity
    }

    public void SetOverCharged()
    {
        overChargeFeedbacks.PlayFeedbacks();
        damage = ClumpOverchargedDamage.Value;
    }
    
    #endregion
    
    #region Collision
    public void HandleCollisions(Collider[] colliders)
    {
        //, Vector3 normal
        if (colliders.Length < 1 || hasCollided) return;
        
        hasCollided = true;
        foreach (var collider in colliders) {
            GameObject hitObj = collider.gameObject;

            var hitEnemy = hitObj.IsInLayerMask(CollisionMaskEnemy);
            var hitInteractable = hitObj.IsInLayerMask(CollisionMaskInteractable);
            //Hit enemy
            if(hitEnemy)
            {
                EnemyController enemyScript = hitObj.GetComponentInChildren<EnemyController>();
                
                //If no enemy controller found, look for hurt proxy
                if (enemyScript == null)
                    enemyScript = hitObj.GetComponent<EnemyHurtProxy>().EnemyController;
                
                //Apply Damage/Knockback
                Vector3 knockBackDir = (enemyScript.transform.position - transform.position).normalized;
                float knockBackForce = currentVelocity.magnitude * 2f;
                enemyScript.DamageEnemy(ClumpDamage.Value, knockBackDir, true, knockBackForce);
                enemyHitId.Value = enemyScript.gameObject.GetInstanceID();
                
                enemyScript.OnDeath += OnParentEnemyDeath;
            }

            //Hit Interactable
            if (hitInteractable)
            {
                var interactableScript = collider.GetComponent<InteractionReceiver>();
                if (interactableScript == null)
                    interactableScript = collider.GetComponent<InteractionReceiverProxy>()?.InteractionReceiver;
                
                if (interactableScript != null)
                    interactableScript.ReceiveMeatClumpHitInteraction(new MeatClumpHitPayload()
                    {
                        hitDir = currentVelocity.normalized,
                        origin = transform.position
                    });

                var damageableScript = collider.GetComponent<Damageable>();
                if ((!damageableScript?.SafeIsUnityNull()) ?? false)
                    damageableScript.OnDeathe += OnParentEnemyDeath;
            }

            if (hitEnemy || hitInteractable)
            {
                if (shaderUpdater.SafeIsUnityNull())
                    shaderUpdater.StartSplat(-meshTransform.forward);
                
                impactFeedbacks.PlayFeedbacks();
                transform.SetParent(hitObj.transform);
                OnCollideWithEnemy.Invoke();
            }
        }
    
        //Static object hit
        if (shaderUpdater != null) shaderUpdater.StartSplat(-meshTransform.forward);
        OnCollideWithStatic.Invoke();
        TimeUtils.SetTimeout(ClumpDestroyTime.Value, () =>
        {
            if (this != null) Destroy(gameObject);
        });
    }

    private void OnParentEnemyDeath()
    {
        if (this.SafeIsUnityNull()) return;

        if (DropMeatOnParentEnemyDeath)
        {
            SpawnManager.SpawnInfo spawnInfo = new SpawnManager.SpawnInfo()
            {
                position = transform.position
            };
            SpawnMeatEvent.Raise(spawnInfo);
        }
        Destroy(gameObject);
    }
    #endregion
    
    #region Movement
    private void Move(Vector3 deltaDistance)
    {
        transform.position += deltaDistance;
        meshTransform.forward = currentVelocity.normalized;    //Point mesh in direction of velocity
    }
    #endregion
}
