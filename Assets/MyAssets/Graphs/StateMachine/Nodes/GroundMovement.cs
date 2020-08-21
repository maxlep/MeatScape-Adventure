using UnityEngine;
using UnityEngine.UI;


public class GroundMovement : PlayerStateNode
{
    public float MoveSpeed = 10f;
    public float Gravity = 10f;
    public float Acceleration = 0.025f;
    public float Deacceleration = 0.015f;

    private Transform cameraTrans;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
        playerController.onStartUpdateVelocity += UpdateVelocity;
        cameraTrans = playerController.GetCameraTrans();
    }
    
    public override void Enter()
    {
        base.Enter();
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
    }

    private void UpdateVelocity(Vector3 currentVelocity)
    {
        if (!isActiveState) return;
        
        // This is called when the motor wants to know what its velocity should be right now
        Vector2 camForward = new Vector2(cameraTrans.forward.x, cameraTrans.forward.z).normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * playerController.moveInput;
        Vector3 moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
        Vector3 targetVelocity = moveDirection * MoveSpeed;
        Vector3 startVelocity = currentVelocity;
        Vector3 newVel = Vector3.zero;

        //If move input is basically zero, de-accelerate
        if (Mathf.Approximately(moveDirection.sqrMagnitude, 0f))
        {
            //currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-deacceleration * deltaTime));
            currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref newVel, Deacceleration);
        }
        else
        {
            //currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-acceleration * deltaTime));
            currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref newVel, Acceleration);
        }

        //Gravity
        if (!playerController.MaintainingGround())
            currentVelocity.y = startVelocity.y - (Gravity * Time.deltaTime);
        
        playerController.SetPlayerVelocity(currentVelocity);
    }
}
