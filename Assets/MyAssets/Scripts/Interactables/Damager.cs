using System;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Sirenix.Utilities;
using UnityEngine;

namespace MyAssets.Scripts.Interactables
{
    public class Damager : MonoBehaviour
    {
        [SerializeField] private IntReference DamageAmount;
        [SerializeField] private FloatReference DamageActiveTime;
        [SerializeField] private LayerMask CollisionMaskEnemy;
        [SerializeField] private LayerMask CollisionMaskPlayer;
        [SerializeField] private LayerMask CollisionMaskInteractable;

        private float _damageStartTime;
        private bool _damageActive = false;

        private void Start()
        {
            _damageStartTime = Time.time;
            _damageActive = true;
        }

#region Collision

        private void OnTriggerEnter(Collider collider)
        {
            if (!_damageActive) return;

            var elapsedTime = Time.time - _damageStartTime;
            if (elapsedTime >= DamageActiveTime.Value)
            {
                _damageActive = false;
                return;
            }
            
            GameObject hitObj = collider.gameObject;

            var hitPlayer = hitObj.IsInLayerMask(CollisionMaskPlayer);
            var hitEnemy = hitObj.IsInLayerMask(CollisionMaskEnemy);
            var hitInteractable = hitObj.IsInLayerMask(CollisionMaskInteractable);

            if (hitPlayer)
            {
                PlayerController playerScript = collider.GetComponent<PlayerController>();
                if (playerScript != null)
                {
                    playerScript.Damage(DamageAmount.Value, Vector3.zero, 0f);
                }
            }
            
            if (hitEnemy)
            {
                EnemyController enemyScript = hitObj.GetComponentInChildren<EnemyController>();
                
                //If no enemy controller found, look for hurt proxy
                if (enemyScript == null)
                    enemyScript = hitObj.GetComponent<EnemyHurtProxy>().EnemyController;
                
                //Apply Damage/Knockback
                Vector3 knockBackDir = (enemyScript.transform.position - transform.position).normalized;
                float knockBackForce = 0;//currentVelocity.magnitude * 2f;
                enemyScript.DamageEnemy(DamageAmount.Value, knockBackDir, true, knockBackForce);
                // enemyHitId.Value = enemyScript.gameObject.GetInstanceID();
            }

            if (hitInteractable)
            {
                var interactableScript = collider.GetComponent<InteractionReceiver>();
                if (interactableScript == null)
                    interactableScript = collider.GetComponent<InteractionReceiverProxy>()?.InteractionReceiver;
                
                if (interactableScript != null)
                    interactableScript.ReceiveMeatClumpHitInteraction(new MeatClumpHitPayload());

                // var damageableScript = collider.GetComponent<Damageable>();
                // if ((!damageableScript?.SafeIsUnityNull()) ?? false)
                //     damageableScript.OnDeathe += OnParentEnemyDeath;
            }
        
            //Static object hit
            // TimeUtils.SetTimeout(ClumpDestroyTime.Value, () =>
            // {
            //     if (this != null) Destroy(gameObject);
            // });
        }
        
        #endregion
    }
}