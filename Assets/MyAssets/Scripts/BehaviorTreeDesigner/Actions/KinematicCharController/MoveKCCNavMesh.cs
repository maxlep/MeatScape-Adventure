using System.Diagnostics;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

[TaskCategory("KinematicCharacterController")]
[TaskDescription("Moves KCC towards a target using NavMesh.")]
public class MoveKCCNavMesh : Action
{
	public SharedFloatReference MoveSpeed;
	public SharedVector3 NewVelocity;
	public SharedQuaternion NewRotation;
	public SharedTransform ThisTransform;
	public SharedTransform Target;
	public SharedEnemyController EnemyController;
	public SharedNavMeshAgent Agent;
	
	private NavMeshPath path;
	

	public override void OnStart()
	{
		base.OnStart();
		path = new NavMeshPath();
		
	}

	public override TaskStatus OnUpdate()
	{
		//TODO: Dont calulcate path every cycle
		Agent.Value.SetDestination(Target.Value.position);
		path = Agent.Value.path;
		if (path.status != NavMeshPathStatus.PathComplete)
			path = null;
		else
		{
			for (int i = 0; i < path.corners.Length - 1; i++)
				Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
		}


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
		Vector3 dirToTarget;
		if (path != null && path.corners.Length > 1)
			dirToTarget = (path.corners[1] - transform.position).xoz().normalized;
		else
			dirToTarget = Vector3.zero;

		NewVelocity.Value = dirToTarget * MoveSpeed.Value.Value;
	}

	public void UpdateRotation()
	{
		Vector3 dirToTarget = (Target.Value.position - transform.position).normalized;
		
		if (!Mathf.Approximately(dirToTarget.xoz().magnitude, 0f))
			NewRotation.Value = Quaternion.LookRotation(dirToTarget.xoz(), Vector3.up);
	}
}