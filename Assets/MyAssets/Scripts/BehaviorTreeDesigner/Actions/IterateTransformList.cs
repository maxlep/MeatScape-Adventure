using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Unity/Transform")]
[TaskDescription("Iterate transform list and return next transform")]
public class IterateTransformList : Action
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The Transform List to iterate")]
    public SharedTransformList transformList;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("The next index to return from list")]
    public SharedInt currentIndex = 0;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("The next transform in list")]
    [RequiredField]
    public SharedTransform storeResult;

    public override TaskStatus OnUpdate()
    {
        storeResult.Value = transformList.Value[currentIndex.Value];

        currentIndex.Value++;

        //For now, just wrap around
        if (currentIndex.Value >= transformList.Value.Count)
            currentIndex.Value = 0;

        return TaskStatus.Success;
    }

    public override void OnReset()
    {
        currentIndex = 0;
    }
}