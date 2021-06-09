using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[TaskCategory("AStarPro")]
[TaskDescription("Knocksback an smooth path Astar agent using CharController.")]
public class SmoothPathKnockback : Action
{
    public SharedTransform AgentTransform;
    public SharedVector3 KnockBackDirection;
    public SharedFloat KnockbackTime;
    public SharedFloat KnockbackSpeed;
	
    private CharacterController charController;
    private SmoothAIPath smoothAIPath;
    private float knockBackStartTime;
    private float currentSpeed;
    private float prevSpeed;

    public override void OnStart()
    {
        charController = AgentTransform.Value.GetComponentInChildren<CharacterController>();
        smoothAIPath = AgentTransform.Value.GetComponent<SmoothAIPath>();
        smoothAIPath.enabled = false;
        knockBackStartTime = Time.time;
        currentSpeed = KnockbackSpeed.Value;
        prevSpeed = KnockbackSpeed.Value;
    }
	
    public override void OnEnd()
    {
        smoothAIPath.enabled = true;
    }

    public override TaskStatus OnUpdate()
    {
        if (knockBackStartTime + KnockbackTime.Value > Time.time)
        {
            HandleKnockBack();
            return TaskStatus.Running;
        }

        return TaskStatus.Success;
    }

    private void HandleKnockBack()
    {
        currentSpeed = Mathf.Lerp(prevSpeed, 0f,  Time.deltaTime/KnockbackTime.Value);
        var velocity = currentSpeed * KnockBackDirection.Value;
		
        charController.Move(velocity * Time.deltaTime);
        prevSpeed = currentSpeed;
    }
}
