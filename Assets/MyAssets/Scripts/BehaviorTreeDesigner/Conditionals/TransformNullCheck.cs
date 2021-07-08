using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using UnityEngine;

[TaskCategory("Unity/Transform")]
[TaskDescription("Check if a Transform is null")]
public class TransformNullCheck : BehaviorDesigner.Runtime.Tasks.Conditional
{
    
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The transform to check")]
    public SharedTransform transform;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("Check against null or not null")]
    public SharedBool isNull;


    public override TaskStatus OnUpdate()
    {
        bool transformIsNull = transform.Value == isNull.Value;

        if (transformIsNull == isNull.Value)
            return TaskStatus.Success;
        
        return TaskStatus.Failure;
    }

    public override void OnReset()
    {
        transform = null;
        isNull = true;
    }
}
