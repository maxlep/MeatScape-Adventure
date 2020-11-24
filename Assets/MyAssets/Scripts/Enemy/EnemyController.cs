﻿using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using KinematicCharacterController;
using MoreMountains.Feedbacks;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class EnemyController : MonoBehaviour, ICharacterController
{
    [SerializeField] private TransformSceneReference playerTransformReference;
    [SerializeField] private KinematicCharacterMotor characterMotor;
    [SerializeField] private BehaviorTree behaviorTree;
    [SerializeField] private LayerMapper layerMapper;
    [SerializeField, SuffixLabel("m/s^2", Overlay = true)] private float Gravity = 10f;
    [SerializeField] private int MaxHealth = 1;
    [SerializeField] private MMFeedbacks damageFeedbacks;
    [SerializeField] private MMFeedbacks deathFeedbacks;
    
    public Vector3 SetNewVelocity
    {
        set => newVelocity = value;
    } 
    
    public Quaternion SetNewRotation
    {
        set => newRotation = value;
    }
    
    public delegate void OnDeath_();
    public event OnDeath_ OnDeath;
    public delegate void _OnStartUpdateVelocity(Vector3 currentVelocity);
    public delegate void _OnStartUpdateRotation(Quaternion currentRotation);
    public event _OnStartUpdateVelocity onStartUpdateVelocity;
    public event _OnStartUpdateRotation onStartUpdateRotation;

    private bool isAlive;
    private int health;
    private Vector3 newVelocity;
    private Quaternion newRotation;

    public void Initialize(List<Transform> patrolPoints)
    {
        behaviorTree.SetVariableValue("CurrentPath", patrolPoints);
        behaviorTree.SetVariableValue("PlayerTransform", playerTransformReference.Value);
        behaviorTree.EnableBehavior();
    }

    #region Unity Methods

    private void Awake() {
        characterMotor.CharacterController = this;
        health = MaxHealth;
        isAlive = true;
    }

    private void LateUpdate() {
        if (isAlive && health <= 0)
        {
            isAlive = false;
            KillEnemy();
        }
    }

    #endregion

    #region CharacterController Methods

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (onStartUpdateVelocity != null) onStartUpdateVelocity.Invoke(currentVelocity);

        float currentGravity = (!characterMotor.GroundingStatus.IsStableOnGround) ? Gravity : 0f;

        currentVelocity = (newVelocity + Vector3.down * currentGravity) * Convert.ToInt32(isAlive);
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (onStartUpdateRotation != null) onStartUpdateRotation.Invoke(currentRotation);
        currentRotation = newRotation;
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

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
        OnDeath?.Invoke();
        KinematicCharacterSystem.UnregisterCharacterMotor(characterMotor);
        // EffectsManager.Instance?.SpawnParticlesAtPoint(deathParticles, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void DamageEnemy(int dmg) {
        health -= dmg;
        if (deathFeedbacks != null) damageFeedbacks?.PlayFeedbacks();
    }

    #endregion
}
