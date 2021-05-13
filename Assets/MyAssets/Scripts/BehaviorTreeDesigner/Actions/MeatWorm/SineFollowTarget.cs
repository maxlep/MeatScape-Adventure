using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using UnityEngine;

[TaskCategory("MeatWorm")]
[TaskDescription("Sets direction for target follow in sine wave")]
public class SineFollowTarget : Action
{
    public SharedTransform Target;
    public SharedTransform controllerTransform;
    public SharedTransform headTransform;

    private MeatWormController meatWormController;

    public override void OnAwake()
    {
        base.OnAwake();
        meatWormController = controllerTransform.Value.GetComponentInChildren<MeatWormController>();
    }
    
    public override TaskStatus OnUpdate()
    {
        UpdateDirection();
        return TaskStatus.Running;
    }

    private void UpdateDirection()
    {
        Vector3 dirToTarget = (Target.Value.position - headTransform.Value.position).xoz().normalized;

        meatWormController.SetTargetDirection(dirToTarget);
    }
}
