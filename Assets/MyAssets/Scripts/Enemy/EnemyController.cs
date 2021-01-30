using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using KinematicCharacterController;
using MoreMountains.Feedbacks;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using Pathfinding;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private TransformSceneReference playerTransformReference;
    [SerializeField] private BehaviorTree behaviorTree;
    [SerializeField, SuffixLabel("m/s^2", Overlay = true)] private float Gravity = 10f;
    [SerializeField] private int MaxHealth = 1;
    [SerializeField] private MMFeedbacks damageFeedbacks;
    [SerializeField] private MMFeedbacks deathFeedbacks;
    [SerializeField] private GameObject dropOnDeath;

    [SerializeField] private UnityEvent OnDeathEvent;

    public delegate void OnDeath_();
    public event OnDeath_ OnDeath;


    private bool isAlive;
    private int health;
    private Vector3 newVelocity;
    private Quaternion newRotation;

    public void Initialize(List<Transform> patrolPoints)
    {
        behaviorTree.SetVariableValue("CurrentPath", patrolPoints);
    }

    #region Unity Methods

    private void Awake() {
        health = MaxHealth;
        isAlive = true;
        behaviorTree.SetVariableValue("PlayerTransform", playerTransformReference.Value);
        behaviorTree.EnableBehavior();
    }

    private void LateUpdate() {
        if (isAlive && health <= 0)
        {
            isAlive = false;
            KillEnemy();
        }
    }

    #endregion
    

    #region Death Methods

    private void KillEnemy() {
        behaviorTree.DisableBehavior(true);
        if (deathFeedbacks != null)
        {
            deathFeedbacks.PlayFeedbacks();
        }
        else
        {
            FinishKill();
        }
    }

    public void FinishKill()
    {
        behaviorTree.DisableBehavior();
        OnDeath?.Invoke();
        GameObject.Instantiate(dropOnDeath, transform.position + Vector3.up * 3f, Quaternion.identity);
        OnDeathEvent?.Invoke();
        // EffectsManager.Instance?.SpawnParticlesAtPoint(deathParticles, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void DamageEnemy(int dmg) {
        health -= dmg;
        if (deathFeedbacks != null) damageFeedbacks?.PlayFeedbacks();
    }

    #endregion
}
