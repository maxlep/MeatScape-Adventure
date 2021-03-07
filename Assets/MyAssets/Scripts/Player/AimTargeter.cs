using Cinemachine;
using MyAssets.ScriptableObjects.Events;
using Sirenix.Utilities;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

namespace MyAssets.Scripts.Player
{
    public class AimTargeter : MonoBehaviour
    {
        [SerializeField] private CameraSceneReference camera;
        [SerializeField] private TransformSceneReference firePoint;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private CinemachineFreeLook freeLookCam;
        [SerializeField] private CinemachineVirtualCamera lockOnVirtualCam;
        [SerializeField] private CinemachineTargetGroup targetGroup;
        [SerializeField] private GameObject targetingReticlePrefab;
        [SerializeField] private LayerMapper layerMapper;
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private LayerMask obstructionLayerMask;
        [SerializeField] private float maxRange;
        [SerializeField] private int colliderLimit = 64;
        [SerializeField] private IntVariable enemyHitId;
        [SerializeField] private GameEvent lockOnStartEvent;
        [SerializeField] private GameEvent lockOnStopEvent;
        
        
        public Transform CurrentTarget => currentTarget?.transform;
        
        private Collider[] targetableColliders;
        
        private Collider currentTarget, lastTarget;
        private float currentWeight;
        private bool lockedOn;

        private CinemachineFramingTransposer framingTransposer;
        
        private GameObject targetingReticle;
        private LTDescr targettingMoveTween;
        
        #region Lifecycle
        private void Awake()
        {
            InputManager.Instance.onLockOn += () => ToggleLockOnMode(!lockedOn);
            targetableColliders = new Collider[colliderLimit];
            lastTarget = null;
            currentTarget = null;
            currentWeight = 0;
            targetingReticle = Instantiate<GameObject>(targetingReticlePrefab);
            targetingReticle.SetActive(false);
            framingTransposer = lockOnVirtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        
        private void Update()
        {
            HandleUpdateTarget();
            
            //Make LockOnCam distance equal the current free look middle rig radius
            framingTransposer.m_CameraDistance = freeLookCam.m_Orbits[1].m_Radius;
        }
        #endregion
        
        #region Update Positions/Target

        private void HandleUpdateTarget()
        {
            currentWeight = GetTargetWeight(currentTarget);
            if (Mathf.Approximately(currentWeight, 0))
            {
                lastTarget = null;
                currentTarget = null;
                currentWeight = 0;
                ToggleLockOnMode(false);
            }
            
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, maxRange, targetableColliders, enemyLayerMask);
            
            for (int i = 0; i < numColliders; i++)
            {
                if (lockedOn)
                    break;
                
                var current = targetableColliders[i];
                if(current.gameObject.GetInstanceID() == enemyHitId.Value) {
                    currentTarget = current;
                    break;
                }
                float weight = GetTargetWeight(current);
                
                if (weight > currentWeight)
                {
                    currentTarget = current;
                    currentWeight = weight;
                }
            }

            if (!currentTarget.SafeIsUnityNull())
            {
                RaycastHit hit = default(RaycastHit);
                bool hitObstruction = Physics.Raycast
                (
                    firePoint.Value.position, 
                    (currentTarget.bounds.center - firePoint.Value.position).normalized,
                    out hit, maxRange, 
                    obstructionLayerMask
                );
                if (!hitObstruction || hit.transform.gameObject.layer == layerMapper.GetLayer(LayerEnum.Enemy))
                {
                    if (!GameObject.ReferenceEquals(currentTarget, lastTarget))
                    {
                        if (!lockedOn) ChangeTarget();
                    }
                    else
                    {
                        FollowTarget();
                    }
                    
                    lastTarget = currentTarget;
                    targetingReticle.SetActive(true);
                    return;
                }
                // Debug.Log($"Curr weight: {currentWeight}, Curr name: {currentTarget?.name}, Num colls: {numColliders}, Screen position: {currentScreenSpacePosition}");
            }
            
            lastTarget = currentTarget;
            targetingReticle.SetActive(false);
        }
        private void FollowTarget()
        {
            if (targettingMoveTween == null)
            {
                targetingReticle.transform.position = GetTargetPosition(currentTarget);
            }
        }
        
        private void ChangeTarget()
        {
            if (targettingMoveTween != null)
            {
                LeanTween.cancel(targettingMoveTween.id);
            }

            var startPosition = lastTarget.SafeIsUnityNull() ? transform.position : targetingReticle.transform.position;
            targettingMoveTween = LeanTween.value
            (
                targetingReticle.gameObject,
                (Vector3 value) =>
                {
                    targetingReticle.transform.position = value;
                }, 
                startPosition, 
                GetTargetPosition(currentTarget), 
                0.25f
            ).setOnComplete(() =>
            {
                targettingMoveTween = null;
            }).setEase(LeanTweenType.easeInOutCubic);
        }
        #endregion

        #region LockOn

        private void ToggleLockOnMode(bool lockOnEnabled)
        {
            if (!lockOnEnabled)
            {
                ToggleLockOnCamera(false);
                lockOnStopEvent.Raise();
                lockedOn = false;
                return;
            }
            
            //Don't LockOn if no current target
            if (currentTarget == null)
            {
                ToggleLockOnCamera(false);
                lockOnStopEvent.Raise();
                lockedOn = false;
                return;
            }
            
            ToggleLockOnCamera(true);
            lockOnStartEvent.Raise();
            lockedOn = true;

            CinemachineTargetGroup.Target playerTarget = new CinemachineTargetGroup.Target();
            playerTarget.weight = 1f;
            playerTarget.radius = 0f;
            playerTarget.target = playerController.transform;
            
            CinemachineTargetGroup.Target enemyTarget = new CinemachineTargetGroup.Target();
            enemyTarget.weight = 1f;
            enemyTarget.radius = 0f;
            enemyTarget.target = currentTarget.transform;
            
            targetGroup.m_Targets = new[] {playerTarget, enemyTarget};
        }
        
        private void ToggleLockOnCamera(bool lockOnEnabled)
        {
            //Only toggle if needed, for performance
            if (lockOnVirtualCam.enabled != lockOnEnabled) lockOnVirtualCam.enabled = lockOnEnabled;
            if (freeLookCam.enabled == lockOnEnabled) freeLookCam.enabled = !lockOnEnabled;
        }

        #endregion
        
        #region Calculate
        private float GetTargetWeight(Collider target)
        {
            if (target.SafeIsUnityNull()) return 0;
            
            var targetPos = target.bounds.center;
            
            var playerToTargetRay = targetPos - transform.position;
            var playerAlignment = Vector3.Dot(transform.forward, playerToTargetRay);
            
            var sqrRange = Mathf.Pow(maxRange, 2);
            var distFac = playerToTargetRay.sqrMagnitude / sqrRange;
            
            var camTransform = camera.Value.transform;
            var camToTargetRay = targetPos - camTransform.position;
            var camAlignment = Vector3.Dot(camTransform.forward, camToTargetRay);
            
            if (distFac > 1) return 0;
            
            // Debug.Log($"{camToTargetRay.sqrMagnitude}|{playerToTargetRay.sqrMagnitude}|{sqrRange}|{distFac}|{target.name}");
            
            return (camAlignment * (1 - distFac)) * (1 - distFac) * (playerAlignment * (1 - distFac));
        }
        
        private static Vector3 GetTargetPosition(Collider target)
        {
            var bounds = target.bounds;
            var center = bounds.center;
            center.y += bounds.extents.y;
            return center;
        }
        #endregion

        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, maxRange);
        }
    }
}