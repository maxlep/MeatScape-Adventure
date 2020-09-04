using Cinemachine.Utility;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class AirMovement : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference MoveSpeed;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference Acceleration;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference Deacceleration;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference TimeToJumpApex;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference MaxJumpHeight;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference FallMultiplier;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference LowJumpDrag;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference MaxFallSpeed;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private BoolReference JumpPressed;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private Vector2Reference MoveInput;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private Vector3Reference NewVelocityOut;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private QuaternionReference NewRotationOut;
    

    private float gravity;
    private float releaseJumpTime;
    private Transform cameraTrans;
    private Vector3 moveDirection;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    public override void RuntimeInitialize()
    {
        base.RuntimeInitialize();
        cameraTrans = playerController.GetCameraTrans();
        gravity = -(2 * MaxJumpHeight.Value) / Mathf.Pow(TimeToJumpApex.Value, 2);
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
        Vector3 targetVelocity = moveDirection * MoveSpeed.Value;
        Vector3 startVelocity = currentVelocity;
        Vector3 newVel = Vector3.zero;

        //If move input is basically zero, de-accelerate
        if (Mathf.Approximately(moveDirection.sqrMagnitude, 0f))
        {
            currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity,
                ref newVel, Deacceleration.Value);
        }
        else
        {
            currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity,
                ref newVel, Acceleration.Value);
        }
        
        currentVelocity.y = startVelocity.y;

        if (currentVelocity.y <= 0)  //Falling
        {
            currentVelocity.y += gravity * (FallMultiplier.Value - 1) * Time.deltaTime;
        }
        else if (currentVelocity.y > 0 && !JumpPressed.Value)    //Short jump
        {
            currentVelocity.y -= LowJumpDrag.Value * Time.deltaTime;
            currentVelocity.y += gravity * Time.deltaTime;
        }
        else
        {
            currentVelocity.y += gravity * Time.deltaTime;
        }
        if (currentVelocity.y < -Mathf.Abs(MaxFallSpeed.Value))   //Cap Speed
        {
            currentVelocity.y = -Mathf.Abs(MaxFallSpeed.Value);
        }

        NewVelocityOut.Value = currentVelocity;
    }
    
    private void UpdateRotation(Quaternion currentRotation)
    {
        if (moveDirection.AlmostZero()) return;
        currentRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        NewRotationOut.Value = currentRotation;
    }

    private void InitMoveDirection()
    {
        Vector2 camForward = new Vector2(cameraTrans.forward.x, cameraTrans.forward.z).normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }
}
