using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using UnityEngine;

[TaskCategory("Combat")]
[TaskDescription("Rotate toward the target")]
public class TrackTarget : Action
{
    public SharedTransform Target;
    public SharedTransform TransformToRotate;
    public SharedTransform IkTarget;
    public SharedTransform IkTargetIdle;
    public SharedFloat Duration;
    public SharedFloat DeltaRotation;

    private float startTime;

    public override void OnStart()
    {
        base.OnStart();
        startTime = Time.time;
    }

    public override TaskStatus OnUpdate()
    {
        Vector3 dirToTarget = (Target.Value.position - TransformToRotate.Value.position).xoz().normalized;

        Quaternion rotationToTarget = Quaternion.LookRotation(dirToTarget, Vector3.up);
        TransformToRotate.Value.rotation = Quaternion.RotateTowards(TransformToRotate.Value.rotation,
            rotationToTarget, DeltaRotation.Value);

        //Reset to idle IK position
        IkTarget.Value.position = IkTargetIdle.Value.position;
        
        //Only track for a set duration before return success
        if (startTime + Duration.Value < Time.time)
            return TaskStatus.Success;

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        base.OnEnd();
    }
}