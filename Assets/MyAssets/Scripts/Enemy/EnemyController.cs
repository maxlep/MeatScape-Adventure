﻿using System;
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
    [SerializeField] protected GameObject dropOnDeath;

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
        isAlive = true;
        behaviorTree.SetVariableValue("PlayerTransform", playerTransformReference.Value);
        behaviorTree.EnableBehavior();
        
        //Check for patrol point helper
        patrolPointHelper = GetComponent<PatrolPointHelper>();
        if (patrolPointHelper != null)
            behaviorTree.SetVariableValue("CurrentPath", patrolPointHelper.PatrolPoints);

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
        GameObject.Instantiate(dropOnDeath, transform.position + Vector3.up * 3f, Quaternion.identity);
        OnDeathEvent?.Invoke();
        // EffectsManager.Instance?.SpawnParticlesAtPoint(deathParticles, transform.position, Quaternion.identity);
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
    
    public virtual void DamageEnemy(int dmg, Vector3 knockBackDirection, bool applyKnockBack = true) {
        health -= dmg;
        if (damageFeedbacks != null) damageFeedbacks?.PlayFeedbacks();

        if (applyKnockBack)
        {
            SharedVariable KnockBackDirVar = behaviorTree.GetVariable("KnockBackDirection");
            if (KnockBackDirVar != null) KnockBackDirVar.SetValue(knockBackDirection);
            behaviorTree.SendEvent("TookDamage");
        }
    }

    #endregion
}
