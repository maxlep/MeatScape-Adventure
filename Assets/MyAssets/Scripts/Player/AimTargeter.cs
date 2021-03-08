using Cinemachine;
using MyAssets.ScriptableObjects.Events;
using Sirenix.Utilities;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using UnityEngine;

namespace MyAssets.Scripts.Player
{
    public class AimTargeter : MonoBehaviour
    {
        [SerializeField] private CameraSceneReference camera;
        [SerializeField] private TransformSceneReference firePoint;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Transform lockOnCamTarget;
        [SerializeField] private CinemachineFreeLook freeLookCam;
        [SerializeField] private CinemachineVirtualCamera lockOnVirtualCam;
        [SerializeField] private CinemachineTargetGroup targetGroup;
        [SerializeField] private GameObject targetingReticlePrefab;
        [SerializeField] private LayerMapper layerMapper;
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private LayerMask obstructionLayerMask;
        [SerializeField] private float maxRange;
        [SerializeField] private float timeToLoseLockOnTarget = 2f;
        [SerializeField] private int colliderLimit = 64;
        [SerializeField] private IntVariable enemyHitId;
        [SerializeField] private GameEvent lockOnStartEvent;
        [SerializeField] private GameEvent lockOnStopEvent;
        
        
        public Transform CurrentTarget => currentTarget?.transform;
        
        private Collider[] targetableColliders;
        
        private Collider currentTarget, lastTarget, cycleTargetRight, cycleTargetLeft;
        private float currentWeight, currentCycleRightAngle, currentCycleLeftAngle;
        private bool lockedOn;
        private float lastSeenTargetTime;

        private CinemachineFramingTransposer framingTransposer;
        
        private GameObject targetingReticle;
        private LTDescr targettingMoveTween;
        
        #region Lifecycle
        private void Awake()
        {
            InputManager.Instance.onLockOn += () => UpdateLockOn(!lockedOn);
            InputManager.Instance.onCycleTargetRight += () => TryCycleTarget(true);
            InputManager.Instance.onCycleTargetLeft += () => TryCycleTarget(false);
            targetableColliders = new Collider[colliderLimit];
            lastTarget = null;
            currentTarget = null;
            currentWeight = 0;
            lastSeenTargetTime = Mathf.NegativeInfinity;
            ClearCycleTargets();
            targetingReticle = Instantiate<GameObject>(targetingReticlePrefab);
            targetingReticle.SetActive(false);
            framingTransposer = lockOnVirtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        
        private void Update()
        {
            HandleUpdateTarget();
            
            //Make LockOnCam distance equal the current free look middle rig radius
            //So it changes with player size as well
            framingTransposer.m_CameraDistance = freeLookCam.m_Orbits[1].m_Radius;
        }
        #endregion
        
        #region Update Positions/Target

        private void HandleUpdateTarget()
        {
            ClearCycleTargets();
            
            //Update current target weight
            currentWeight = GetTargetWeight(currentTarget);
            if (Mathf.Approximately(currentWeight, 0))
            {
                lastTarget = null;
                currentTarget = null;
                currentWeight = 0;
                UpdateLockOn(false);
            }

            Vector2 playerToTarget = Vector2.zero;
            
            if (lockedOn)
                playerToTarget = (currentTarget.bounds.center - playerController.transform.position).xz();
            
            //TODO: What if 1 enemy has multiple colliders? Right now assuming each enemy has 1
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, maxRange, targetableColliders, enemyLayerMask);
            for (int i = 0; i < numColliders; i++)
            {
                var current = targetableColliders[i];
                float weight = GetTargetWeight(current);

                //If locked on, update cycle targets
                if (lockedOn && !Mathf.Approximately(weight, 0f))
                {
                    //Dont consider the currentTarget for lock on cycle
                    if (current.Equals(currentTarget)) continue;
                    
                    var playerToCurrent = (current.bounds.center - playerController.transform.position).xz();
                    var signedAngle = -Vector2.SignedAngle(playerToTarget, playerToCurrent);
                    if (signedAngle >= 0f && signedAngle < currentCycleRightAngle)
                    {
                        currentCycleRightAngle = signedAngle;
                        cycleTargetRight = current;
                    }
                    else if (signedAngle < 0f && signedAngle > currentCycleLeftAngle)
                    {
                        currentCycleLeftAngle = signedAngle;
                        cycleTargetLeft = current;
                    }
                }
                //If not locked on, handle auto-lock target based on weights
                else
                {
                    if (weight > currentWeight)
                    {
                        currentTarget = current;
                        currentWeight = weight;
                    }
                }
            }

            //If target null, disable reticle
            if (currentTarget.SafeIsUnityNull())
            {
                lastTarget = currentTarget;
                targetingReticle.SetActive(false);
                return;
            }

            //If new target, move reticle
            if (!GameObject.ReferenceEquals(currentTarget, lastTarget))
            {
                MoveReticleToTarget();
            }
            else
            {
                FollowTarget();
            }
            
            lastTarget = currentTarget;
            targetingReticle.SetActive(true);
        }
        private void FollowTarget()
        {
            if (targettingMoveTween == null)
            {
                targetingReticle.transform.position = GetTargetPosition(currentTarget);
            }
        }
        
        private void MoveReticleToTarget()
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

        private void UpdateLockOn(bool lockOnEnabled)
        {
            if (!lockOnEnabled)
            {
                ToggleLockOnCamera(false);
                lockOnStopEvent.Raise();
                cycleTargetRight = null;
                cycleTargetLeft = null;
                currentCycleRightAngle = Mathf.Infinity;
                currentCycleLeftAngle = Mathf.NegativeInfinity;
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
            playerTarget.target = lockOnCamTarget;
            
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

        private void TryCycleTarget(bool isRight)
        {
            if (!lockedOn) return;
            
            if (isRight && cycleTargetRight != null)
            {
                currentTarget = cycleTargetRight;
                MoveReticleToTarget();
                UpdateLockOn(true);
            }
            else if (!isRight && cycleTargetLeft)
            {
                currentTarget = cycleTargetLeft;
                MoveReticleToTarget();
                UpdateLockOn(true);
            }
        }

        private void ClearCycleTargets()
        {
            cycleTargetRight = null;
            cycleTargetLeft = null;
            currentCycleRightAngle = Mathf.Infinity;
            currentCycleLeftAngle = Mathf.NegativeInfinity;
        }

        #endregion
        
        #region Calculate
        private float GetTargetWeight(Collider target)
        {
            if (target.SafeIsUnityNull()) return 0;
            bool isLockOnTarget = lockedOn && target.Equals(currentTarget);
            
            //TODO: Consider the last hit enemy here
            // if(current.gameObject.GetInstanceID() == enemyHitId.Value) {
            //     currentTarget = current;
            //     break;
            // }
            
            RaycastHit hit = default(RaycastHit);
            bool hitObstruction = Physics.Raycast
            (
                camera.Value.transform.position, 
                (target.bounds.center - camera.Value.transform.position).normalized,
                out hit, maxRange, 
                obstructionLayerMask
            );
            if (hitObstruction)
                hitObstruction &= !hit.collider.Equals(target); //Dont consider the target an obstruction

            if (hitObstruction)
            {
                if (!isLockOnTarget)
                    return 0;

                //Return 0 if this is the lock on target and haven't seen in while
                if (Time.time - lastSeenTargetTime > timeToLoseLockOnTarget)
                    return 0;
            }

            if (!hitObstruction && isLockOnTarget)
                lastSeenTargetTime = Time.time;
            
            var targetPos = target.bounds.center;
            
            var playerToTargetRay = targetPos - transform.position;
            var playerAlignment = Vector3.Dot(transform.forward, playerToTargetRay);
            playerAlignment = Mathf.Max(.01f, playerAlignment);    //Prevent 0 value that would stop LockOn
            
            var sqrRange = Mathf.Pow(maxRange, 2);
            var distFac = playerToTargetRay.sqrMagnitude / sqrRange;
            
            var camTransform = camera.Value.transform;
            var camToTargetRay = targetPos - camTransform.position;
            var camAlignment = Vector3.Dot(camTransform.forward, camToTargetRay);
            
            if (distFac > 1) return 0;
            
            // Debug.Log($"{camToTargetRay.sqrMagnitude}|{playerToTargetRay.sqrMagnitude}|{sqrRange}|{distFac}|{target.name}");
            
            return Mathf.Max(0f,(camAlignment * (1 - distFac)) * (1 - distFac) * (playerAlignment * (1 - distFac)));
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