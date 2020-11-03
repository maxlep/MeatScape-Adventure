using System.Diagnostics;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

[TaskCategory("KinematicCharacterController")]
[TaskDescription("Moves KCC towards a target.")]
public class MoveKCC : Action
{
	public SharedFloatReference MoveSpeed;
	public SharedVector3 NewVelocity;
	public SharedQuaternion NewRotation;
	public SharedTransform ThisTransform;
	public SharedTransform Target;
	public SharedEnemyController EnemyController;
	
	

	public override void OnStart()
	{
		base.OnStart();
	}

	public override TaskStatus OnUpdate()
	{
		UpdateVelocity();
		UpdateRotation();
		return TaskStatus.Success;
	}

	public override void OnEnd()
	{
		base.OnEnd();

	}

	public void UpdateVelocity()
	{
		Vector3 dirToTarget = (Target.Value.position - transform.position).normalized;

		NewVelocity.Value = dirToTarget * MoveSpeed.Value.Value;
	}

	public void UpdateRotation()
	{
		Vector3 dirToTarget = (Target.Value.position - transform.position).normalized;
		
		if (!Mathf.Approximately(dirToTarget.xoz().magnitude, 0f))
			NewRotation.Value = Quaternion.LookRotation(dirToTarget.xoz(), Vector3.up);
	}
}