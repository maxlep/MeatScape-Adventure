using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;

[TaskCategory("Unity/Transform")]
[TaskDescription("Returns the distance between two Transforms.")]
public class DistanceTransforms : Action
{
	[BehaviorDesigner.Runtime.Tasks.Tooltip("The first Transform")]
	public SharedTransform firstTransform;
	[BehaviorDesigner.Runtime.Tasks.Tooltip("The second Transform")]
	public SharedTransform secondTransform;
	[BehaviorDesigner.Runtime.Tasks.Tooltip("The distance between Transforms Position.")]
	[RequiredField]
	public SharedFloat storeResult;
	
	public override TaskStatus OnUpdate()
	{
		storeResult.Value = Vector3.Distance(firstTransform.Value.position, secondTransform.Value.position);
		return TaskStatus.Success;
	}

	public override void OnReset()
	{
		storeResult.Value = 0;
	}
}