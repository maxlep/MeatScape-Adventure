using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using UnityEngine;

[TaskCategory("Unity/Transform")]
[TaskDescription("Compares distance (using square distance) between two transforms against provided float using the selected operation")]
public class TransformDistanceCheck : Conditional
{
    public enum Operation
    {
        LessThan,
        LessThanOrEqualTo,
        EqualTo,
        NotEqualTo,
        GreaterThanOrEqualTo,
        GreaterThan
    }

    [BehaviorDesigner.Runtime.Tasks.Tooltip("The operation to perform")]
    public Operation operation;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The first transform")]
    public SharedTransform transform1;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The second transform")]
    public SharedTransform transform2;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The float to compare distance against")]
    public SharedFloat distToCheck;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("Only compare X and Z when checking distance")]
    public SharedBool IgnoreYValue;

    public override TaskStatus OnUpdate()
    {
        float sqrDistance;
        if (IgnoreYValue.Value)
            sqrDistance = (transform1.Value.position.xoz() - transform2.Value.position.xoz()).sqrMagnitude;
        else
            sqrDistance = (transform1.Value.position - transform2.Value.position).sqrMagnitude;
        
        float distToCheckSqr = Mathf.Pow(distToCheck.Value, 2f);
        
        switch (operation) {
            case Operation.LessThan:
                return sqrDistance < distToCheckSqr ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.LessThanOrEqualTo:
                return sqrDistance <= distToCheckSqr ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.EqualTo:
                return UnityEngine.Mathf.Approximately(sqrDistance, distToCheckSqr) ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.NotEqualTo:
                return !UnityEngine.Mathf.Approximately(sqrDistance, distToCheckSqr) ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.GreaterThanOrEqualTo:
                return sqrDistance >= distToCheckSqr ? TaskStatus.Success : TaskStatus.Failure;
            case Operation.GreaterThan:
                return sqrDistance > distToCheckSqr ? TaskStatus.Success : TaskStatus.Failure;
        }
        return TaskStatus.Failure;
    }

    public override void OnReset()
    {
        operation = Operation.LessThan;
        transform1.Value = null;
        transform2.Value = null;
        distToCheck.Value = 0;
    }
}
