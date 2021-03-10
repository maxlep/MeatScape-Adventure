using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

[TaskCategory("Util")]
[TaskDescription("Repeats sequence a random number of times")]
[TaskIcon("{SkinColor}RepeaterIcon.png")]
public class RandomRepeater : Decorator
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The minimum possible number of times the child task will run")]
    public SharedInt minCount = 1;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("The maximum possible number of times the child task will run")]
    public SharedInt maxCount = 2;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("Should the task return if the child task returns a failure")]
    public SharedBool endOnFailure;

    // The number of times the child taks will be run;
    private int count = 0;
    // The number of times the child task has been run.
    private int executionCount = 0;
    // The status of the child after it has finished running.
    private TaskStatus executionStatus = TaskStatus.Inactive;

    public override void OnStart() {
        count = Random.Range(minCount.Value, maxCount.Value);
    }

    public override bool CanExecute()
    {
        // Continue executing until we've reached the count or the child task returned failure and we should stop on a failure.
        return (executionCount < count) && (!endOnFailure.Value || (endOnFailure.Value && executionStatus != TaskStatus.Failure));
    }

    public override void OnChildExecuted(TaskStatus childStatus)
    {
        // The child task has finished execution. Increase the execution count and update the execution status.
        executionCount++;
        executionStatus = childStatus;
    }

    public override void OnEnd()
    {
        // Reset the variables back to their starting values.
        executionCount = 0;
        executionStatus = TaskStatus.Inactive;
    }

    public override void OnReset()
    {
        // Reset the public properties back to their original values.
        count = 0;
        endOnFailure = true;
    }
}
