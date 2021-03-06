﻿using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using Pathfinding;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Combat")]
[TaskDescription("Performs lunge attack using AStar Seeker and CharController.")]
public class LungeAttack : Action
{
    public LayerMapper layerMapper;
    public SharedTransform Target;
    public SharedTransform AgentTransform;
    public SharedGameObject AnimTarget;
    public SharedFloat ChargeTime;
    public SharedFloat GroundCheckDelay;
    public SharedFloat ForwardLungeVelocity;
    public SharedFloat UpwardLungeVelocity;
    public SharedFloat Gravity;
    public SharedBool IsLunging;

    private CharacterController charController;
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private Animator animator;
    private GameObject prevGameObject;
    private float chargeStartTime;
    private float lungeStartTime;

    private Collider targetCollider;
    private Rigidbody targetRigidbody;
    
    public override void OnAwake()
    {
        chargeStartTime = Mathf.NegativeInfinity;
        lungeStartTime = Mathf.NegativeInfinity;
        destinationSetter = AgentTransform.Value.GetComponentInChildren<AIDestinationSetter>();
        charController = AgentTransform.Value.GetComponentInChildren<CharacterController>();
        aiPath = AgentTransform.Value.GetComponentInChildren<AIPath>();
        
    }

    public override void OnStart()
    {
        //Change to enemy Layer so able to collide with player
        AgentTransform.Value.gameObject.layer = layerMapper.GetLayer(LayerEnum.Enemy);
        
        //Init animator and detect if target obj changed
        var currentGameObject = GetDefaultGameObject(AnimTarget.Value);
        if (currentGameObject != prevGameObject && currentGameObject != null) {
            animator = currentGameObject.GetComponent<Animator>();
            prevGameObject = currentGameObject;
        }
        
        if (animator != null) animator.SetTrigger("PrepareLunge");
        
        destinationSetter.target = transform;
        aiPath.enabled = false;
        IsLunging.Value = false;
        chargeStartTime = Time.time;
        lungeStartTime = Mathf.NegativeInfinity;
        targetCollider = Target.Value.GetComponent<Collider>();
        targetRigidbody = Target.Value.GetComponent<Rigidbody>();
    }

    public override void OnEnd()
    {
        aiPath.enabled = true;
        
        //Change to enemyAgebt Layer so dont collide with player
        AgentTransform.Value.gameObject.layer = layerMapper.GetLayer(LayerEnum.EnemyAgent);
    }

    public override TaskStatus OnUpdate()
    {
        //If still charging
        if (chargeStartTime + ChargeTime.Value > Time.time)
            return TaskStatus.Running;

        if (!IsLunging.Value)
            StartLungeAttack();

        if (LungeAttacking())
            return TaskStatus.Running;

        return TaskStatus.Success;
    }
    
    public override void OnReset()
    {
        Target.Value = null;
        charController = null;
        destinationSetter = null;
        IsLunging.Value = false;
        chargeStartTime = Mathf.NegativeInfinity;
        lungeStartTime = Mathf.NegativeInfinity;
        targetCollider = null;
        targetRigidbody = null;
    }

    private void StartLungeAttack()
    {
        IsLunging.Value = true;
        lungeStartTime = Time.time;
        float distToTarget = (targetCollider.bounds.center - transform.position).magnitude;
        float lungeTimeToTarget = (ForwardLungeVelocity.Value + UpwardLungeVelocity.Value) / distToTarget;
        Vector3 dirToTarget = ((targetCollider.bounds.center + (targetRigidbody.velocity * lungeTimeToTarget)) - transform.position).normalized;

        Vector3 forwardForce = ForwardLungeVelocity.Value * dirToTarget.xoz();
        Vector3 upwardForce = UpwardLungeVelocity.Value * Vector3.up;
        Vector3 totalForce = forwardForce + upwardForce;
        
        //Add "Force" to the char controller to get it moving and face target
        AgentTransform.Value.rotation = Quaternion.LookRotation(dirToTarget, Vector3.up);
        charController.Move(totalForce * Time.deltaTime);
        
        if (animator != null) animator.SetTrigger("StartLunge");
    }

    private bool LungeAttacking()
    {
        //Handle lunge velocity/gravity
        Vector3 gravity = Gravity.Value * Vector3.down * Time.deltaTime;
        Vector3 currentVelocity = charController.velocity;
        Vector3 newVelocity = currentVelocity + gravity;
        
        //Stop lunge attack when grounded and waited delay
        if (lungeStartTime + GroundCheckDelay.Value < Time.time &&
            charController.isGrounded)
        {
            IsLunging.Value = false;
            if (animator != null) animator.SetTrigger("FinishLunge");
            return false;
        }

        charController.Move(newVelocity * Time.deltaTime);
        return true;
    }
}
