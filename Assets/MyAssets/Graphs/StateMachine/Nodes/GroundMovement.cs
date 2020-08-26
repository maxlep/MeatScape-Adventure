﻿using Cinemachine.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class GroundMovement : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] public float MoveSpeed = 10f;
    [FoldoutGroup("")] [LabelWidth(120)] public float Acceleration = 0.025f;
    [FoldoutGroup("")] [LabelWidth(120)] public float Deacceleration = 0.015f;
    [FoldoutGroup("")] [LabelWidth(120)] public Vector2Reference MoveInput;

    private Transform cameraTrans;
    private Vector3 moveDirection;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
        cameraTrans = playerController.GetCameraTrans();
    }
    
    public override void Enter()
    {
        base.Enter();
        playerController.onStartUpdateVelocity += UpdateVelocity;
        playerController.onStartUpdateRotation += UpdateRotation;
        InitMoveDirection(); //Call to force update moveDir in case updateRot called b4 updateVel
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void ExecuteFixed()
    {
        base.ExecuteFixed();
    }

    public override void Exit()
    {
        base.Exit();
        playerController.onStartUpdateVelocity -= UpdateVelocity;
        playerController.onStartUpdateRotation -= UpdateRotation;
    }

    private void UpdateVelocity(Vector3 currentVelocity)
    {
        // This is called when the motor wants to know what its velocity should be right now
        Vector2 camForward = new Vector2(cameraTrans.forward.x, cameraTrans.forward.z).normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
        Vector3 targetVelocity = moveDirection * MoveSpeed;
        Vector3 startVelocity = currentVelocity;
        Vector3 newVel = Vector3.zero;

        //If move input is basically zero, de-accelerate
        if (Mathf.Approximately(moveDirection.sqrMagnitude, 0f))
            currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref newVel, Deacceleration);
        else
            currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref newVel, Acceleration);

        playerController.SetPlayerVelocity(currentVelocity);
    }

    private void UpdateRotation(Quaternion currentRotation)
    {
        if (moveDirection.AlmostZero()) return;
        currentRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        playerController.SetPlayerRotation(currentRotation);
    }
    
    private void InitMoveDirection()
    {
        Vector2 camForward = new Vector2(cameraTrans.forward.x, cameraTrans.forward.z).normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }
}
