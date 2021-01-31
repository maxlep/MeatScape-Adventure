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
    public SharedFloat FireForce;
    public SharedFloat ShootDelay;

    private float lastShootTime;

    public override void OnAwake()
    {
        lastShootTime = Mathf.NegativeInfinity;
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
        FireForce.Value = 0f;
        ShootDelay.Value = 0f;
        lastShootTime = Mathf.NegativeInfinity;
    }

    private void Fire()
    {
        lastShootTime = Time.time;
        
        //Fire from firepoint
        Vector3 firePos = FirePoint.Value.position;
        Vector3 dirToPlayer = (Target.Value.position - firePos).normalized;
        Quaternion fireRot = Quaternion.LookRotation(dirToPlayer);
        
        var projectile = GameObject.Instantiate(Projectile.Value, firePos, fireRot);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        projectileRb.AddForce(dirToPlayer * FireForce.Value);
    }

    
}
