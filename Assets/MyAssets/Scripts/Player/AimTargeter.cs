using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Den.Tools;
using MyAssets.ScriptableObjects.Events;
using Sirenix.Utilities;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
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
        [SerializeField] private LayerMask targetableMask;
        [SerializeField] private LayerMask obstructionLayerMask;
        [SerializeField] private float maxRange = 90f;
        [SerializeField] private float maxRangeNotFacing = 15f;
        [SerializeField] private float aimReticleYOffset;
        [SerializeField] private float timeToLoseLockOnTarget = 2f;
        [SerializeField] private float reticleMoveTime = .15f;
        [SerializeField] private int colliderLimit = 64;
        [SerializeField] private IntVariable enemyHitId;
        [SerializeField] private GameEvent lockOnStartEvent;
        [SerializeField] private GameEvent lockOnStopEvent;
        [SerializeField] private FloatVariable lockOnCameraAimOffsetYDistance;
        [SerializeField] private FloatVariable lockOnCameraAimOffsetYScale;
        [SerializeField] private IntVariable lockOnTargetCurrentDistance;
        [SerializeField] private BoolReference lockedOn;
        [SerializeField] private BoolReference hasTarget;
        [SerializeField] private TransformSceneReference currentTargetSceneReference;


        public Transform CurrentTarget => currentTarget?.transform;
        
        private Collider[] targetableColliders;
        
        private Collider currentTarget, lastTarget, cycleTargetRight, cycleTargetLeft;
        private float currentWeight, currentRightWeight, currentLeftWeight, currentCycleRightAngle, currentCycleLeftAngle;
        private float lastSeenTargetTime;

        private CinemachineFramingTransposer framingTransposer;
        private CinemachineGroupComposer groupComposer;
        
        private GameObject targetingReticle;
        private LTDescr targettingMoveTween;

#region Lifecycle
        private void Awake()
        {
            InputManager.Instance.onLockOn += () => UpdateLockOn(!lockedOn.Value);
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
            groupComposer = lockOnVirtualCam.GetCinemachineComponent<CinemachineGroupComposer>();
        }
        
        private void Update()
        {
            //Remove stale cached weights
#if UNITY_EDITOR
            weightsCache.RemoveWhere(collider => (DateTime.Now - weightsCache[collider].time).Seconds >= weightCacheStaleTimer);
#endif
            
            //Change distance and aim offset with size (indirectly)
            framingTransposer.m_CameraDistance = freeLookCam.m_Orbits[1].m_Radius;
            groupComposer.m_TrackedObjectOffset.y = lockOnCameraAimOffsetYDistance.Value +
                                                    lockOnCameraAimOffsetYScale.Value;
            
            //If locked on and target null (dies), cycle to next best
            if (lockedOn.Value && currentTarget.SafeIsUnityNull())
            {
                if (currentRightWeight > currentLeftWeight)
                    TryCycleTarget(true);
                else 
                    TryCycleTarget(false);
            }
            
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

            if (lockedOn.Value)
                playerToTarget = (currentTarget.bounds.center - playerController.transform.position).xz();

            //TODO: What if 1 enemy has multiple colliders? Right now assuming each enemy has 1
            //Cycle through targets and handle lock on and auto-aim modes
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, maxRange, targetableColliders, targetableMask);
            for (int i = 0; i < numColliders; i++)
            {
                var current = targetableColliders[i];
                float weight = GetTargetWeight(current);
                
                if (lockedOn.Value && !Mathf.Approximately(weight, 0f))
                {
                    //Dont consider the currentTarget for lock on cycle
                    if (current.Equals(currentTarget)) continue;
                    UpdateCycleTargets(current, playerToTarget);
                }
                //If not locked on, handle update auto-lock target based on weights
                else if (!lockedOn.Value && weight > currentWeight)
                {
                    currentTarget = current;
                    currentWeight = weight;
                }
            }

            //If target null, disable reticle
            if (currentTarget.SafeIsUnityNull() || !currentTarget.enabled)
            {
                lastTarget = currentTarget;
                targetingReticle.SetActive(false);
                UpdateLockOn(false);
                return;
            }

            //If new target, move reticle
            if (!GameObject.ReferenceEquals(currentTarget, lastTarget))
                MoveReticleToTarget();
            else
                FollowTarget();

            lastTarget = currentTarget;
            targetingReticle.SetActive(true);
        }

        private void LateUpdate()
        {
            if (currentTarget != null)
            {
                currentTargetSceneReference.Value = currentTarget.transform;
                hasTarget.Value = true;
            }

            else
            {
                currentTargetSceneReference.Value = null;
                hasTarget.Value = false;
            }
                

            if (lockedOn.Value)
            {
                Vector3 playerToTargetHorizontal = currentTarget.transform.position.xoz() - transform.position.xoz();
                lockOnTargetCurrentDistance.Value = Mathf.FloorToInt(playerToTargetHorizontal.magnitude);
            }
        }

        #endregion
        
        #region Update Positions/Target
        
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
                targettingMoveTween = null;
            }

            if (lastTarget.SafeIsUnityNull())
            {
                targetingReticle.transform.position = GetTargetPosition(currentTarget);
                return;
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
                reticleMoveTime
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
                lockedOn.Value = false;
                return;
            }
            
            //Don't LockOn if no current target
            if (currentTarget == null)
            {
                ToggleLockOnCamera(false);
                lockOnStopEvent.Raise();
                lockedOn.Value = false;
                return;
            }
            
            ToggleLockOnCamera(true);
            lockOnStartEvent.Raise();
            lockedOn.Value = true;
            
            //Update target groups transforms
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
            if (!lockedOn.Value) return;
            
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

        private void UpdateCycleTargets(Collider target, Vector2 playerToTarget)
        {
            var playerToCurrent = (target.bounds.center - playerController.transform.position).xz();
            var signedAngle = -Vector2.SignedAngle(playerToTarget, playerToCurrent);
            if (signedAngle >= 0f && signedAngle < currentCycleRightAngle)
            {
                currentCycleRightAngle = signedAngle;
                currentRightWeight = GetTargetWeight(target);
                cycleTargetRight = target;
            }
            else if (signedAngle < 0f && signedAngle > currentCycleLeftAngle)
            {
                currentCycleLeftAngle = signedAngle;
                currentLeftWeight = GetTargetWeight(target);
                cycleTargetLeft = target;
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
            if (target == null) return 0;

#if UNITY_EDITOR
            weightsCache.TryAdd(target, new WeightStatus());
            weightsCache[target].Reset();
#endif
            
            if (target.SafeIsUnityNull()) return 0;
            bool isLockOnTarget = lockedOn.Value && target.Equals(currentTarget);
#if UNITY_EDITOR
            weightsCache[target].isLockOnTarget = isLockOnTarget;
#endif
            
            //TODO: Consider the last hit enemy here
            // if(current.gameObject.GetInstanceID() == enemyHitId.Value) {
            //     currentTarget = current;
            //     break;
            // }

            //If target not within camera bounds
            if (!CameraUtils.IsVisible(target.bounds.center, target.bounds.extents, camera.Value))
            {
#if UNITY_EDITOR
                weightsCache[target].inCamera = false;
#endif
                return 0;
            }
            
            //Check for obstructions
            RaycastHit hit = default(RaycastHit);
            bool hitObstruction = Physics.Raycast
            (
                camera.Value.transform.position, 
                (target.bounds.center - camera.Value.transform.position).normalized,
                out hit, maxRange, 
                obstructionLayerMask,
                QueryTriggerInteraction.Ignore
            );
            if (hitObstruction)
                hitObstruction &= !hit.collider.Equals(target); //Dont consider the target an obstruction

            if (hitObstruction)
            {
#if UNITY_EDITOR
                weightsCache[target].isObstructed = true;
#endif
                
                if (!isLockOnTarget)
                    return 0;

                //Return 0 if this is the lock on target and haven't seen in while
                if (Time.time - lastSeenTargetTime > timeToLoseLockOnTarget)
                    return 0;
            }

            //Update last seen LockOn Target
            if (!hitObstruction && isLockOnTarget)
                lastSeenTargetTime = Time.time;
            
            var targetPos = target.bounds.center;
            
            var playerToTargetRay = targetPos - transform.position;
            var playerAlignment = Vector3.Dot(transform.forward.normalized, playerToTargetRay.normalized);
            playerAlignment = Mathf.Clamp01(playerAlignment);
#if UNITY_EDITOR
            weightsCache[target].playerAlignmentFactor = playerAlignment;
#endif
            
            var sqrRange = Mathf.Pow(maxRange, 2);
            var maxRangeFac = playerToTargetRay.sqrMagnitude / sqrRange;
            var maxRangeNotFacingFac = Mathf.Clamp01(playerToTargetRay.sqrMagnitude / Mathf.Pow(maxRangeNotFacing, 2));
            // Already squared from distance calculation
#if UNITY_EDITOR
            weightsCache[target].playerDistanceFactor = maxRangeFac;
#endif
            
            var camTransform = camera.Value.transform;
            var camToTargetRay = targetPos - camTransform.position;
            var camAlignment = Mathf.Clamp01(Vector3.Dot(camTransform.forward.normalized, playerToTargetRay.normalized));
#if UNITY_EDITOR
            weightsCache[target].cameraAlignmentFactor = camAlignment;
#endif
            
            if (maxRangeFac > 1)
            {
#if UNITY_EDITOR
                weightsCache[target].inRange = false;
#endif
                return 0;
            };
            
            // Debug.Log($"{camToTargetRay.sqrMagnitude}|{playerToTargetRay.sqrMagnitude}|{sqrRange}|{distFac}|{target.name}");

            var weight =
                Mathf.Max(
                    0f, 
                    (camAlignment * (1 - maxRangeFac))
                        * (1 - maxRangeFac)
                        * (playerAlignment * (1 - maxRangeFac))
                );
#if UNITY_EDITOR
            weightsCache[target].playerAlignmentFactor = (playerAlignment * Math.Max(maxRangeFac, 0.5f)) * 0.3f;
            weightsCache[target].playerDistanceFactor = (1 - maxRangeNotFacingFac) * 0.3f;
            weightsCache[target].cameraAlignmentFactor = (camAlignment * camAlignment * (1 - maxRangeFac)) * 0.3f;
            weight =
                weightsCache[target].playerAlignmentFactor
                + weightsCache[target].playerDistanceFactor
                + weightsCache[target].cameraAlignmentFactor;
#else
            weight =
                (playerAlignment * distFac * distFac)
                + ((1 - distFac) * (1 - distFac) * (1 - distFac))
                + (camAlignment * (1 - distFac));
#endif
            weight = Mathf.Clamp01(weight);
#if  UNITY_EDITOR

            weightsCache[target].totalWeight = weight;
#endif
            return weight;
        }
        
        private Vector3 GetTargetPosition(Collider target)
        {
            var bounds = target.bounds;
            var center = bounds.center;
            center.y += bounds.extents.y;
            center.y += aimReticleYOffset;
            return center;
        }
        #endregion
        
        // Debug
#if UNITY_EDITOR
        private float weightCacheStaleTimer = 3f;
        private class WeightStatus
        {
            public DateTime time;
            public float totalWeight;
            public float playerAlignmentFactor;
            public float playerDistanceFactor;
            public float cameraAlignmentFactor;
            public bool inRange;
            public bool isObstructed;
            public bool inCamera;
            public bool isLockOnTarget;

            public WeightStatus()
            {
                Reset();
            }

            public void Reset()
            {
                time = DateTime.Now;
                totalWeight = 0;
                inRange = true;
                isObstructed = false;
                inCamera = true;
                isLockOnTarget = false;
            }
        }
        private Dictionary<Collider, WeightStatus> weightsCache = new Dictionary<Collider, WeightStatus>();
#endif

        private void OnDrawGizmos() {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, maxRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, maxRangeNotFacing);
            
            foreach (var kv in weightsCache)
            {
                var pos = kv.Key.transform.position;
                var rad = 30 * Mathf.Clamp01(kv.Value.totalWeight);
                // Gizmos.color = Color.yellow;
                // Gizmos.DrawSphere(pos, rad);
                Color color = Color.blue;

                if (!kv.Value.inCamera || !kv.Value.inRange || kv.Value.isObstructed)
                {
                    color = Color.red;
                }
                else if (kv.Value.isLockOnTarget)
                {
                    color = Color.green;
                }
                else
                {
                    color = Color.Lerp(Color.yellow, color, kv.Value.cameraAlignmentFactor);
                }
                Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Pixels, pos, rad, color);
            }
        }
    }
}