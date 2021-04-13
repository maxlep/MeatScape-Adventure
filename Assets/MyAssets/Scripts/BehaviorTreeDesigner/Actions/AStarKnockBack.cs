using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Den.Tools;
using Pathfinding;

[TaskCategory("AStarPro")]
[TaskDescription("Knocksback an Astar agent using CharController.")]
public class AStarKnockBack : Action
{
	public SharedTransform AgentTransform;
	public SharedVector3 KnockBackDirection;
	public SharedFloat KnockbackTime;
	public SharedFloat KnockbackSpeed;
	
	private CharacterController charController;
	private AIPath aiPath;
	private float knockBackStartTime;
	private float currentSpeed;
	private float prevSpeed;

	public override void OnStart()
	{
		charController = AgentTransform.Value.GetComponentInChildren<CharacterController>();
		aiPath = AgentTransform.Value.GetComponent<AIPath>();
		aiPath.enabled = false;
		knockBackStartTime = Time.time;
		currentSpeed = KnockbackSpeed.Value;
		prevSpeed = KnockbackSpeed.Value;
	}
	
	public override void OnEnd()
	{
		aiPath.enabled = true;
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