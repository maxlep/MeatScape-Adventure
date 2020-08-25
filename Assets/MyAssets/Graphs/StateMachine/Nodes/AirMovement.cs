using Cinemachine.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class AirMovement : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] public float MoveSpeed = 10f;
    [FoldoutGroup("")] [LabelWidth(120)] public float Acceleration = 0.025f;
    [FoldoutGroup("")] [LabelWidth(120)] public float Deacceleration = 0.015f;
    [FoldoutGroup("")] [LabelWidth(120)] public float timeToJumpApex = .4f;
    [FoldoutGroup("")] [LabelWidth(120)] public float maxJumpHeight = 4f;
    [FoldoutGroup("")] [LabelWidth(120)] public float fallMultiplier = 7f;
    [FoldoutGroup("")] [LabelWidth(120)] public float lowJumpDrag = .025f;
    [FoldoutGroup("")] [LabelWidth(120)] public float maxFallSpeed = 10f;

    private float gravity;
    private float releaseJumpTime;
    private Transform cameraTrans;
    private Vector3 moveDirection;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
        cameraTrans = playerController.GetCameraTrans();
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
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
        Vector2 rotatedMoveInput = rotOffset * playerController.MoveInput;
        moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
        Vector3 targetVelocity = moveDirection * MoveSpeed;
        Vector3 startVelocity = currentVelocity;
        Vector3 newVel = Vector3.zero;

        //If move input is basically zero, de-accelerate
        if (Mathf.Approximately(moveDirection.sqrMagnitude, 0f))
            currentVelocity = Vector3.SmoothDamp(startVelocity, targetVelocity, ref newVel, Deacceleration);
        else
            currentVelocity = Vector3.SmoothDamp(startVelocity, targetVelocity, ref newVel, Acceleration);
        
        currentVelocity.y = startVelocity.y;

        if (currentVelocity.y <= 0)  //Falling
        {
            currentVelocity.y += gravity * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (currentVelocity.y > 0 && !playerController.JumpPressed)    //Short jump
        {
            currentVelocity.y -= lowJumpDrag;
            currentVelocity.y += gravity * Time.deltaTime;
        }
        else
        {
            currentVelocity.y += gravity * Time.deltaTime;
        }
        if (currentVelocity.y < -Mathf.Abs(maxFallSpeed))   //Cap Speed
        {
            currentVelocity.y = -Mathf.Abs(maxFallSpeed);
        }
        
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
        Vector2 rotatedMoveInput = rotOffset * playerController.MoveInput;
        moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }
}
