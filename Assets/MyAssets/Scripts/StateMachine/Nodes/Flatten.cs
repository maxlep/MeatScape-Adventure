using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class Flatten : PlayerStateNode {
	
	#region Outputs

	[HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
	protected Vector3Reference NewVelocityOut;
	
	[HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
	protected QuaternionReference NewRotationOut;

	#endregion
	
	#region Events

	[HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
	[Required]
	private GameEvent FlattenStartEvent;
	
	[HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
	[Required]
	private GameEvent FlattenStopEvent;

	#endregion

	public override void Enter()
	{
		base.Enter();
		FlattenStartEvent.Raise();
		playerController.onStartUpdateVelocity += UpdateVelocity;
		playerController.onStartUpdateRotation += UpdateRotation;
	}

	private void UpdateVelocity(VelocityInfo velocityInfo)
	{
		//If not grounded yet, keep y velocity going until grounded
		if (!playerController.GroundingStatus.IsStableOnGround)
			NewVelocityOut.Value = velocityInfo.currentVelocity.y * Vector3.one.oyo();
		else
			NewVelocityOut.Value = Vector3.zero;
	}
	
	private void UpdateRotation(Quaternion currentRotation)
	{
		NewRotationOut.Value = currentRotation;
	}

	public override void Exit()
	{
		base.Exit();
		FlattenStopEvent.Raise();
		playerController.onStartUpdateVelocity -= UpdateVelocity;
		playerController.onStartUpdateRotation -= UpdateRotation;

	}
}