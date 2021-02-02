using System;
using BehaviorDesigner.Runtime;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MyAssets.Scripts.Player
{
    public class AimTargeter : MonoBehaviour
    {
        [SerializeField] private CameraSceneReference camera;
        [SerializeField] private TransformSceneReference targetingReticle;
        [SerializeField] private TransformSceneReference firePoint;
        [SerializeField] private LayerMapper layerMapper;
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private LayerMask obstructionLayerMask;
        [SerializeField] private float maxRange;
        [SerializeField] private int colliderLimit = 64;

        public Transform CurrentTarget => currentTarget?.transform;

        private Collider[] targetableColliders;

        private Collider currentTarget;
        private float currentWeight;

        private void Awake()
        {
            targetableColliders = new Collider[colliderLimit];
            currentTarget = null;
            currentWeight = 0;
        }

        private void Update()
        {
            currentWeight = GetTargetWeight(currentTarget);
            if (Mathf.Approximately(currentWeight, 0))
            {
                currentTarget = null;
                currentWeight = 0;
            }
            
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, maxRange, targetableColliders, enemyLayerMask);
            
            for (int i = 0; i < numColliders; i++)
            {
                var current = targetableColliders[i];
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
                bool hitObstruction = Physics.Raycast(firePoint.Value.position, (currentTarget.bounds.center - firePoint.Value.position).normalized,
                    out hit, maxRange, obstructionLayerMask);
                if(!hitObstruction || hit.transform.gameObject.layer == layerMapper.GetLayer(LayerEnum.Enemy)) {
                    var currentScreenSpacePosition = camera.Value.WorldToScreenPoint(currentTarget.bounds.center);
                    targetingReticle.Value.position = currentScreenSpacePosition;
                    targetingReticle.Value.gameObject.SetActive(true);
                    return;
                }
                // Debug.Log($"Curr weight: {currentWeight}, Curr name: {currentTarget?.name}, Num colls: {numColliders}, Screen position: {currentScreenSpacePosition}");
            }
            
            targetingReticle.Value.gameObject.SetActive(false);
        }

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
    }
}