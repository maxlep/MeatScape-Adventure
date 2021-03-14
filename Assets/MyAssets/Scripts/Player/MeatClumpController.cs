using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.ShaderHelpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class MeatClumpController : MonoBehaviour
{
    [SerializeField, Required("Shader Update not set! Splat effect won't trigger!", InfoMessageType.Warning)]
    private MeatClumpShaderUpdater shaderUpdater;
    
    [SerializeField, Required("No Absorb Sound Set!", InfoMessageType.Warning)]
    private AudioClip AbsorbSound;
    
    [SerializeField, Required("No Absorb Particles Set!", InfoMessageType.Warning)]
    private GameObject AbsorbSystem;
    
    [SerializeField] private LayerMapper layerMapper;
    [SerializeField] private FloatReference ClumpScalingFactor;
    [SerializeField] private FloatReference ClumpFalloffDistance;
    [SerializeField] private FloatReference ClumpFalloffSpeed;
    [SerializeField] private FloatReference ClumpFalloffDistanceFactor;
    [SerializeField] private FloatReference CollisionRadius;
    [SerializeField] private FloatReference DragCoefficient;
    [SerializeField] private LayerMask CollisionMask;
    [SerializeField] private UnityEvent OnCollideWithStatic;
    [SerializeField] private IntVariable enemyHitId;

    private float currentSpeed;
    private bool hasCollided = false;
    private bool shouldFallOff = false;
    
    private Vector3 startMovementPoint;
    
    #region Lifecycle
    private void Awake() {
        transform.localScale *= ClumpScalingFactor.Value;
    }

    private void FixedUpdate()
    {
        this.currentSpeed -= this.currentSpeed * DragCoefficient.Value * Time.deltaTime;
        float deltaDistance = this.currentSpeed * Time.deltaTime;

        if (!hasCollided) PreMoveCollisionCheck(deltaDistance);
        if (!hasCollided) Move(deltaDistance);
        if (!hasCollided) PostMoveCollisionCheck();
    }
    #endregion
    
    #region Setters
    public void SetMoving(float speed, Vector3 direction) {
        this.currentSpeed = speed;
        this.transform.forward = direction.normalized;
    }
    #endregion
    
    #region Collision
    private void HandleCollisions(Collider[] colliders, Vector3 normal) {
        if (colliders.Length > 0)
        {
            this.hasCollided = true;
            foreach (var collider in colliders) {
                GameObject hitObj = collider.gameObject;
                
                if(hitObj.layer == layerMapper.GetLayer(LayerEnum.Enemy)) {
                    EnemyController enemyScript = hitObj.GetComponent<EnemyController>();
                    Vector3 knockBackDir = (enemyScript.transform.position - transform.position).normalized;
                    enemyScript.DamageEnemy(1, knockBackDir);
                    enemyHitId.Value = enemyScript.gameObject.GetInstanceID();
                    Destroy(this.gameObject);
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
            out hit, deltaDistance, CollisionMask, QueryTriggerInteraction.Ignore))
        {
            transform.position += (transform.forward * hit.distance);
            
            this.HandleCollisions(new Collider[]{hit.collider}, hit.normal);
        }
    }

    private void PostMoveCollisionCheck()
    {
        //Overlap sphere at final position to check for intersecting colliders
        Collider[] hitColliders =
            (Physics.OverlapSphere(transform.position, CollisionRadius.Value, CollisionMask, QueryTriggerInteraction.Ignore));
        
        this.HandleCollisions(hitColliders, -transform.forward);
    }
    #endregion
    
    #region Movement
    private void Move(float deltaDistance)
    {
        shouldFallOff = Vector3.Distance(transform.position, this.startMovementPoint) >= ClumpFalloffDistance.Value * ClumpFalloffDistanceFactor.Value;
        if(shouldFallOff) {
            float newY = transform.forward.y - (this.ClumpFalloffSpeed.Value * Time.deltaTime * (1 / ClumpFalloffDistanceFactor.Value));
            transform.forward = new Vector3(transform.forward.x, newY, transform.forward.z);
        }
        transform.position += transform.forward * deltaDistance;
    }
    #endregion
}
