using Cinemachine.Utility;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class Movement : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(165)] [SerializeField]
    private bool EnableHorizontalMovement = true;
    
    [HideIf("$zoom")] [LabelWidth(165)] [SerializeField]
    private bool EnableVerticalMovement = false;

    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField]
    private FloatReference MoveSpeed;

    [HideIf("$zoom")] [SuffixLabel("*Pi")] [LabelWidth(120)] [SerializeField]
    private FloatReference TurnSpeed;

    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField]
    private FloatReference Acceleration;

    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField]
    private FloatReference Deacceleration;

    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField]
    private Vector2Reference MoveInput;

    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField]
    private Vector3Reference NewVelocityOut;

    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField]
    private Vector3Reference cachedVelocity;

    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField]
    private QuaternionReference NewRotationOut;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")] [LabelWidth(120)] [SerializeField]
    private FloatReference TimeToJumpApex;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")] [LabelWidth(120)] [SerializeField]
    private FloatReference MaxJumpHeight;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")] [LabelWidth(120)] [SerializeField]
    private FloatReference FallMultiplier;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")] [LabelWidth(120)] [SerializeField]
    private FloatReference LowJumpDrag;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")] [LabelWidth(120)] [SerializeField]
    private FloatReference MaxFallSpeed;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")] [LabelWidth(120)] [SerializeField]
    private BoolReference JumpPressed;

    private Transform cameraTrans;
    private Vector3 moveDirection;

    private float newSpeed;
    private Vector3 newDirection;

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
        SetMoveDirection(); //Call to force update moveDir in case updateRot called b4 updateVel
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
        SetMoveDirection();

        Vector3 horizontalVelocity = Vector3.zero;
        Vector3 verticalVelocity = Vector3.zero;

        if (EnableHorizontalMovement) horizontalVelocity = CalculateHorizontalVelocity(currentVelocity);
        if (EnableVerticalMovement) verticalVelocity = CalculateVerticalVelocity(currentVelocity);

        NewVelocityOut.Value = horizontalVelocity + verticalVelocity;
    }

    private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
    {
        //Rotate current vel towards target vel
        Vector2 horizontalVelocity = currentVelocity.xz();
        Vector2 vel = Vector3.zero;

        Vector2 dir = Vector2.SmoothDamp(horizontalVelocity.normalized, new Vector2(moveDirection.x, moveDirection.z),
            ref vel, TurnSpeed.Value);

        newDirection = new Vector3(dir.x, 0f, dir.y).normalized;

        float speed = 0f;
        float currentSpeed = horizontalVelocity.magnitude;
        float targetSpeed = MoveInput.Value.magnitude * MoveSpeed.Value;

        if (targetSpeed > currentSpeed)
            newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
                ref speed, Acceleration.Value);
        else
            newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
                ref speed, Deacceleration.Value);

        return newDirection * newSpeed;
    }

    private Vector3 CalculateVerticalVelocity(Vector3 currentVelocity)
    {
        float gravity = -(2 * MaxJumpHeight.Value) / Mathf.Pow(TimeToJumpApex.Value, 2);

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

        return currentVelocity.y * Vector3.up;
    }

    private void UpdateRotation(Quaternion currentRotation)
    {
        Vector3 lookDirection = new Vector3(cachedVelocity.Value.x, 0f, cachedVelocity.Value.z);
        if (Mathf.Approximately(lookDirection.magnitude, 0f)) return;
        currentRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        NewRotationOut.Value = currentRotation;
    }
    
    private void SetMoveDirection()
    {
        Vector2 camForward = cameraTrans.forward.xz().normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }

    public override void DrawGizmos()
    {
        if (playerController == null) return;
        
        // set up all static parameters. these are used for all following Draw.Line calls
        Draw.LineGeometry = LineGeometry.Volumetric3D;
        Draw.LineThicknessSpace = ThicknessSpace.Pixels;
        Draw.LineThickness = 6; // 4px wide

        Vector3 startPos = playerController.transform.position;
        Vector3 endPos = playerController.transform.position + newDirection * (newSpeed / MoveSpeed.Value) * 10f;
        Vector3 endPos2 = playerController.transform.position + moveDirection * 10f;
        
        Draw.Line(startPos, endPos, Color.magenta);
        Draw.Line(startPos, endPos2, Color.yellow);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos, .5f, new Color(1f, 0f, 1f , .35f));
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos2, .5f, new Color(1f, 1f, 0f , .35f));
    }
}
