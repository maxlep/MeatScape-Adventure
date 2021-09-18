using MyAssets.ScriptableObjects.Variables;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class CollisionCheck : MonoBehaviour
{
    [SerializeField] private FloatReference CollisionRadius;
    [SerializeField] private bool useOffset = false;
    [ShowIf("useOffset")][SerializeField] private Vector3Reference CollisionOffset;
    [SerializeField] private LayerMask CollisionMask;
    [Tooltip("Enable additional collision check if not colliding")]
    [SerializeField] private bool castCheck;
    [SerializeField] private bool ignoreTriggers = true;
    [SerializeField] private UnityEvent<Collider[]> HandleCollisions;

    private Vector3 previousPositionOffset {
        get {
            return (useOffset ? CollisionOffset.Value : default(Vector3)) + previousPosition;
        }
    }

    private Vector3 positionOffset {
        get {
            return (useOffset ? CollisionOffset.Value : default(Vector3)) + transform.position;
        }
    }

    private Vector3 previousPosition;

    void Awake() {
        previousPosition = transform.position;
    }

    void Update()
    {
        CheckCollisions();
        previousPosition = transform.position;
    }

    private void CheckCollisions() {
      QueryTriggerInteraction triggerInteraction = ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.UseGlobal;
      Collider[] colliders = Physics.OverlapSphere(positionOffset, CollisionRadius.Value, CollisionMask, triggerInteraction);
      if(castCheck) {
        RaycastHit hitEnemy;
        if(Physics.SphereCast(previousPositionOffset, CollisionRadius.Value, (positionOffset - previousPositionOffset).normalized,
              out hitEnemy, Vector3.Distance(previousPositionOffset, positionOffset), CollisionMask, triggerInteraction)) {
          Collider[] newColliders = new Collider[colliders.Length + 1];
          for (var i = 0; i < colliders.Length; i++) {
            newColliders[i] = colliders[i];
          }
          newColliders[newColliders.Length - 1] = hitEnemy.collider;
          colliders = newColliders;
        }
      }
      HandleCollisions.Invoke(colliders);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(positionOffset, CollisionRadius.Value);
    }
}
