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
    [SerializeField] protected TransformSceneReference playerTransformReference;
    [SerializeField] protected BehaviorTree behaviorTree;
    [SerializeField, SuffixLabel("m/s^2", Overlay = true)] protected float Gravity = 10f;
    [SerializeField] protected int MaxHealth = 1;
    [SerializeField] protected MMFeedbacks damageFeedbacks;
    [SerializeField] protected MMFeedbacks deathFeedbacks;
    [SerializeField] protected GameObject destroyOnDeathObj;
    [SerializeField] protected DynamicGameEvent SpawnMeatEvent;
    [SerializeField] protected bool SpawnMeatOnDeath = true;
    [SerializeField] protected HealthBar healthBar;
    

    [SerializeField] protected UnityEvent OnDeathEvent;

    public delegate void OnDeath_();
    public event OnDeath_ OnDeath;
    
    protected bool isAlive;
    protected int health;
    protected PatrolPointHelper patrolPointHelper;

    public virtual void Initialize(List<Transform> patrolPoints)
    {
        behaviorTree.SetVariableValue("CurrentPath", patrolPoints);
    }

    #region Unity Methods

    protected virtual void Awake() {
        health = MaxHealth;
        healthBar.SetMaxHealth(MaxHealth);
        healthBar.gameObject.SetActive(false);
        isAlive = true;
        behaviorTree.SetVariableValue("PlayerTransform", playerTransformReference.Value);
        behaviorTree.EnableBehavior();
        
        //Check for patrol point helper
        // patrolPointHelper = GetComponent<PatrolPointHelper>();
        // if (patrolPointHelper != null)
            // behaviorTree.SetVariableValue("CurrentPath", patrolPointHelper.PatrolPoints);

    }

    protected virtual void LateUpdate() {
        if (isAlive && health <= 0)
        {
            isAlive = false;
            KillEnemy();
        }
    }

    #endregion
    

    #region Death Methods

    protected virtual void KillEnemy() {
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

    public virtual void FinishKill()
    {
        behaviorTree.DisableBehavior();
        OnDeath?.Invoke();
        if (SpawnMeatOnDeath)
        {
            SpawnManager.SpawnInfo spawnInfo = new SpawnManager.SpawnInfo()
            {
                position = transform.position
            };
            SpawnMeatEvent.Raise(spawnInfo);
        }
        OnDeathEvent?.Invoke();
        if (destroyOnDeathObj != null)
            Destroy(destroyOnDeathObj);
        else
            Destroy(this.gameObject);
    }
    
    //Deal damage but no knockback
    public virtual void DamageEnemy(int dmg)
    {
        DamageEnemy(dmg, Vector3.zero, false);
    }
    
    public virtual void DamageEnemy(int dmg, Vector3 knockBackDirection, bool applyKnockBack = true, float knockForce = 50f) {
        health -= dmg;
        if (damageFeedbacks != null) damageFeedbacks?.PlayFeedbacks();
        healthBar.gameObject.SetActive(true);
        healthBar.UpdateHealth(health);

        if (applyKnockBack)
        {
            this.KnockbackEnemy(knockBackDirection, .2f, knockForce);
        }
    }

    public virtual void KnockbackEnemy(Vector3 knockBackDirection, float knockBackTime = .2f, float KnockBackSpeed = 50f) {
        SharedVariable KnockBackDirVar = behaviorTree.GetVariable("KnockBackDirection");
        if (KnockBackDirVar != null) KnockBackDirVar.SetValue(knockBackDirection);
        SharedVariable KnockbackTime = behaviorTree.GetVariable("KnockbackTime");
        if (KnockbackTime != null) KnockbackTime.SetValue(knockBackTime);
        SharedVariable KnockbackSpeed = behaviorTree.GetVariable("KnockbackSpeed");
        if (KnockbackSpeed != null) KnockbackSpeed.SetValue(KnockBackSpeed);
        behaviorTree.SendEvent("TookDamage");
    }

    #endregion
}
