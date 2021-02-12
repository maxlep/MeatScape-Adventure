using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Combat")]
[TaskDescription("Fires projectile.")]
public class FireProjectile : Action
{
    public SharedTransform Target;
    public SharedTransform FirePoint;
    public SharedGameObject Projectile;
    public SharedGameObject AnimTarget;
    public SharedFloat FireSpeed;
    public SharedFloat ShootDelay;

    private float lastShootTime;
    private Collider targetCollider;
    private Rigidbody targetRigidbody;

    public override void OnAwake()
    {
        lastShootTime = Mathf.NegativeInfinity;
    }
    
    public override void OnStart()
    {
        targetCollider = Target.Value.GetComponent<Collider>();
        targetRigidbody = Target.Value.GetComponent<Rigidbody>();
    }

    public override TaskStatus OnUpdate()
    {
        //TODO: If waited shoot delay, fire projectile
        if (lastShootTime + ShootDelay.Value > Time.time)
            return TaskStatus.Running;

        Fire();
        
        return TaskStatus.Success;
    }
    
    public override void OnReset()
    {
        Target.Value = null;
        FirePoint.Value = null;
        AnimTarget.Value = null;
        Projectile.Value = null;
        FireSpeed.Value = 0f;
        ShootDelay.Value = 0f;
        lastShootTime = Mathf.NegativeInfinity;
        targetCollider = null;
        targetRigidbody = null;
    }

    private void Fire()
    {
        lastShootTime = Time.time;
        
        //Fire from firepoint
        Vector3 firePos = FirePoint.Value.position;
        
        Vector3 targetPos = targetCollider.bounds.center;
        Vector3 dirToTarget = (targetPos - firePos);

        
        Vector3 targetVelocity = targetRigidbody.velocity;
        targetVelocity.y = 0;
        float timeToTarget = FireSpeed.Value / dirToTarget.magnitude;
        targetPos += timeToTarget * 2 * targetVelocity;
        
        Vector3 projectileDrop = Physics.gravity * (dirToTarget.y / (Physics.gravity.y + (FireSpeed.Value * (targetPos - firePos).normalized.y)));
        
        Vector3 dirToFire = ((targetPos + -projectileDrop) - firePos).normalized;
        
        Quaternion fireRot = Quaternion.LookRotation(dirToFire);
        
        var projectile = GameObject.Instantiate(Projectile.Value, firePos, fireRot);
        ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
        projectileController.SetMoving(FireSpeed.Value, dirToFire);
    }
}
