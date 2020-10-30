using System.Diagnostics;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

[TaskCategory("KinematicCharacterController")]
[TaskDescription("Moves KCC towards a target using NavMesh.")]
public class MoveKCC : Action
{
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
		
		//EnemyController.Value.onStartUpdateVelocity += UpdateVelocity;
		//EnemyController.Value.onStartUpdateRotation += UpdateRotation;
	}

	public override TaskStatus OnUpdate()
	{
		//TODO: Dont calulcate path every cycle
		Agent.Value.SetDestination(Target.Value.position);
		path = Agent.Value.path;

		for (int i = 0; i < path.corners.Length - 1; i++)
			Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
		
		UpdateVelocity(Vector3.one);
		UpdateRotation(Quaternion.identity);
		return TaskStatus.Success;
	}

	public override void OnEnd()
	{
		base.OnEnd();
		//EnemyController.Value.onStartUpdateVelocity -= UpdateVelocity;
		//EnemyController.Value.onStartUpdateRotation -= UpdateRotation;
	}

	public void UpdateVelocity(Vector3 currentVelocity)
	{
		Vector3 dirToTarget;
		if (path != null && path.corners.Length > 1)
			dirToTarget = (path.corners[1] - transform.position).normalized;
		else
			dirToTarget = Vector3.zero;

		NewVelocity.Value = dirToTarget * 3f;
	}

	public void UpdateRotation(Quaternion currentRotation)
	{
		Vector3 dirToTarget = (Target.Value.position - transform.position).normalized;
		
		if (!Mathf.Approximately(dirToTarget.xoz().magnitude, 0f))
			NewRotation.Value = Quaternion.LookRotation(dirToTarget.xoz(), Vector3.up);
	}
}