using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Combat")]
[TaskDescription("Fires projectile.")]
public class SummonAttack : Action
{
    public SharedTransform FirePoint;
    public SharedGameObject Projectile;

    public override TaskStatus OnUpdate()
    {
        Fire();
        
        return TaskStatus.Success;
    }
    
    public override void OnReset()
    {
        FirePoint.Value = null;
        Projectile.Value = null;
    }

    private void Fire()
    {
        Vector3 firePos = FirePoint.Value.position;

        var projectile = GameObject.Instantiate(Projectile.Value, firePos, Quaternion.identity);
    }
}
