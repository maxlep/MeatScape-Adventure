using MoreMountains.Feedbacks;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.ShaderHelpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using MyAssets.Scripts.Utils;

public class MeatClumpController : MonoBehaviour
{
    [SerializeField, Required("Shader Update not set! Splat effect won't trigger!", InfoMessageType.Warning)]
    private MeatClumpShaderUpdater shaderUpdater;
    
    [SerializeField, Required("No Absorb Sound Set!", InfoMessageType.Warning)]
    private AudioClip AbsorbSound;
    
    [SerializeField, Required("No Absorb Particles Set!", InfoMessageType.Warning)]
    private GameObject AbsorbSystem;
    
    [SerializeField] private LayerMapper layerMapper;
    [SerializeField] private Transform meshTransform;
    [SerializeField] private FloatReference ClumpScalingFactor;
    [SerializeField] private FloatReference ClumpGravityEnableTime;
    [SerializeField] private FloatReference ClumpGravityFactor;
    [SerializeField] private FloatReference ClumpDestroyTime;
    [SerializeField] private FloatReference CollisionRadiusEnvironment;
    [SerializeField] private FloatReference CollisionRadiusEnemy;
    [SerializeField] private FloatReference DragCoefficientHorizontal;
    [SerializeField] private LayerMask CollisionMaskEnvironment;
    [SerializeField] private LayerMask CollisionMaskEnemy;
    [SerializeField] private UnityEvent OnCollideWithStatic;
    [SerializeField] private IntVariable enemyHitId;
    [SerializeField] private MMFeedbacks impactFeedbacks;

    private float startTime;
    private Vector3 currentVelocity;
    private bool hasCollided = false;
    private bool hasGravity = false;
    
    #region Lifecycle
    private void Awake() {
        transform.localScale *= ClumpScalingFactor.Value;
    }

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if(!hasCollided)
        {
            Vector3 horizontalDrag = currentVelocity.xoz() * DragCoefficientHorizontal.Value * Time.deltaTime;
            currentVelocity -= horizontalDrag;
            
            //Enable gravity after clump has been moving for ClumpGravityEnableTime duration.
            if(!hasGravity) hasGravity = Time.time - startTime >= ClumpGravityEnableTime.Value;
            if(hasGravity) currentVelocity += Physics.gravity * ClumpGravityFactor.Value * Time.deltaTime;
            Vector3 deltaDistance = currentVelocity * Time.deltaTime;

            PreMoveCollisionCheck(deltaDistance);
            Move(deltaDistance);
            PostMoveCollisionCheck();
        }
    }
    #endregion
    
    #region Setters
    public void SetMoving(float speed, Vector3 direction) {
        currentVelocity = speed * direction.normalized;
        transform.forward = direction.normalized;
    }
    
    #endregion
    
    #region Collision
    private void HandleCollisions(Collider[] colliders, Vector3 normal)
    {
        if (colliders.Length < 1) return;
        
        hasCollided = true;
        foreach (var collider in colliders) {
            GameObject hitObj = collider.gameObject;
            
            //Hit enemy
            if(hitObj.layer == layerMapper.GetLayer(LayerEnum.Enemy)) {
                EnemyController enemyScript = hitObj.GetComponent<EnemyController>();
                Vector3 knockBackDir = (enemyScript.transform.position - transform.position).normalized;
                float knockBackForce = currentVelocity.magnitude * 2f;
                enemyScript.DamageEnemy(1, knockBackDir, true, knockBackForce);
                enemyHitId.Value = enemyScript.gameObject.GetInstanceID();
                impactFeedbacks.PlayFeedbacks();
                Destroy(gameObject);
                return;
            }

            //Hit Interactable
            if (hitObj.layer == layerMapper.GetLayer(LayerEnum.Interactable))
            {
                var interactableScript = collider.GetComponent<InteractionReceiver>();
                if (interactableScript != null)
                    interactableScript.ReceiveMeatClumpHitInteraction(new MeatClumpHitPayload());
            }
        }
    
        //Static object hit
        if (shaderUpdater != null) shaderUpdater.StartSplat(normal);
        OnCollideWithStatic.Invoke();
        TimeUtils.SetTimeout(ClumpDestroyTime.Value, () =>
        {
            if (this != null) Destroy(gameObject);
        });
        
    }

    private void PreMoveCollisionCheck(Vector3 deltaDistance)
    {
        //SphereCast from current pos to next pos and check for collisions
        //Check for enemy hit
        RaycastHit hitEnemy;
        if (Physics.SphereCast(transform.position, CollisionRadiusEnemy.Value, transform.forward,
            out hitEnemy, deltaDistance.magnitude, CollisionMaskEnemy, QueryTriggerInteraction.Ignore))
        {
            transform.position += (transform.forward * hitEnemy.distance);
            
            HandleCollisions(new Collider[]{hitEnemy.collider}, hitEnemy.normal);
        }
        
        //Check for environment hit
        RaycastHit hitEnvironment;
        if (Physics.SphereCast(transform.position, CollisionRadiusEnvironment.Value, transform.forward,
            out hitEnvironment, deltaDistance.magnitude, CollisionMaskEnvironment, QueryTriggerInteraction.Ignore))
        {
            transform.position += (transform.forward * hitEnvironment.distance);
            
            HandleCollisions(new Collider[]{hitEnvironment.collider}, hitEnvironment.normal);
        }
        
        
    }

    private void PostMoveCollisionCheck()
    {
        //Overlap sphere at final position to check for intersecting colliders.
        
        //Check Enemy hit
        Collider[] hitCollidersEnemy =
            (Physics.OverlapSphere(transform.position, CollisionRadiusEnemy.Value, CollisionMaskEnemy, QueryTriggerInteraction.Ignore));
        
        //Check Environment hit
        Collider[] hitCollidersEnvironment =
            (Physics.OverlapSphere(transform.position, CollisionRadiusEnvironment.Value, CollisionMaskEnvironment, QueryTriggerInteraction.Ignore));
        
        HandleCollisions(hitCollidersEnemy, -meshTransform.forward);
        HandleCollisions(hitCollidersEnvironment, -meshTransform.forward);
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
