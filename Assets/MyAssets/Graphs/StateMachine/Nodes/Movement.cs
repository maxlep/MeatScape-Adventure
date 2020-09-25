using AmplifyShaderEditor;
using Cinemachine.Utility;
using KinematicCharacterController;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class Movement : PlayerStateNode
{
    /**************************************
        * Horizontal Movement *
    **************************************/
    
    [HideIf("$zoom")] [LabelWidth(165)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private bool EnableHorizontalMovement = true;
    
    [HideIf("$zoom")] [LabelWidth(165)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private bool EnableFastTurn = true;
    
    [HideIf("$zoom")] [LabelWidth(165)] [SerializeField] 
    [TabGroup("Vertical Movement")] [Required]
    private bool EnableVerticalMovement = false;
    
    [HideIf("$zoom")] [LabelWidth(165)] [SerializeField] 
    [TabGroup("Horizontal Movement")] [Required]
    private TransformSceneReference PlayerCameraTransform;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private FloatReference MoveSpeed;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private FloatReference TurnSpeed;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private FloatReference RotationDeltaMax;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private FloatReference Acceleration;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private FloatReference Deacceleration;
    
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private Vector2Reference MoveInput;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private Vector3Reference NewVelocityOut;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private Vector3Reference cachedVelocity;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [Required]
    private QuaternionReference NewRotationOut;
    
    /**************************************
             * Fast Turn *
    **************************************/
    
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [ShowIf("$EnableFastTurn")]
    [Required]
    private FloatReference FastTurnSpeed;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [ShowIf("$EnableFastTurn")]
    [Required]
    private FloatReference FastTurnPercentThreshold;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [ShowIf("$EnableFastTurn")]
    [Required]
    private FloatReference FastTurnAngleThreshold;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [ShowIf("$EnableFastTurn")]
    [Required]
    private FloatReference StopFastTurnDeltaAngle;

    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [ShowIf("$EnableFastTurn")]
    [Required]
    private FloatReference FastTurnInputDeadZone;
    
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [ShowIf("$EnableFastTurn")]
    [Required]
    private FloatReference FastTurnBrakeDeacceleration;
    
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [ShowIf("$EnableFastTurn")]
    [Required]
    private FloatReference FastTurnBrakeSpeedThreshold;
    
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Horizontal Movement")] [ShowIf("$EnableFastTurn")]
    [Required]
    private FloatReference MoveInputRequiredDelta;
    
    /**************************************
        * Vertical Movement *
    **************************************/

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")]
    [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    [TabGroup("Vertical Movement")] 
    private FloatReference TimeToJumpApex;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")]
    [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    [TabGroup("Vertical Movement")] 
    private FloatReference MaxJumpHeight;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")]
    [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    [TabGroup("Vertical Movement")]
    private FloatReference FallMultiplier;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")]
    [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    [TabGroup("Vertical Movement")]
    private FloatReference LowJumpDrag;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")]
    [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    [TabGroup("Vertical Movement")]
    private FloatReference MaxFallSpeed;

    [HideIf("$zoom")] [ShowIf("$EnableVerticalMovement")]
    [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    [TabGroup("Vertical Movement")]
    private BoolReference JumpPressed;

    private Vector3 moveDirection;

    private float newSpeed;
    private bool isFastTurning;
    private Vector3 fastTurnStartDir;
    private Vector3 newDirection;
    private Vector3 lastMoveInputDirection = Vector3.zero;
    private Vector3 slopeRight;
    private Vector3 slopeOut;
    

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
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
        CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
        
        /**************************************
         * Determine if Fast Turning
         **************************************/
        
        //Dont allow fast turn on slopes
        if (EnableFastTurn && !GroundingStatus.FoundAnyGround)
            CheckForFastTurn(currentVelocity);
        
        float currentTurnSpeed;
        Vector2 horizontalVelocity = currentVelocity.xz();

        //Update turn speed based on isFastTurning
        if (isFastTurning)
            currentTurnSpeed = FastTurnSpeed.Value;
        else
            currentTurnSpeed = TurnSpeed.Value;
        
        /**************************************
         * Get New Move Direction
         **************************************/

        //Rotate current vel towards target vel to get new direction
        Vector2 dummyVel = Vector3.zero;
        Vector2 dir = Vector2.SmoothDamp(horizontalVelocity.normalized, moveDirection.xz(),
            ref dummyVel, currentTurnSpeed);

        newDirection = new Vector3(dir.x, 0f, dir.y).normalized;
        
        /**************************************
         * Get New Move Speed
         **************************************/

        //Accelerate/DeAccelerate from current Speed to target speed
        float dummySpeed = 0f;
        float currentSpeed = currentVelocity.Flatten().magnitude;
        float targetSpeed = MoveInput.Value.magnitude * MoveSpeed.Value;

        if (targetSpeed > currentSpeed)
            newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
                ref dummySpeed, Acceleration.Value);
        else
            newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
                ref dummySpeed, Deacceleration.Value);
        
        /*********************************************
         * Override Speed and Direction if Fast Turning
         *********************************************/

        //If fast turning, DeAccelerate to 0 to brake
        if (isFastTurning)
        {
            newSpeed = Mathf.SmoothDamp(currentSpeed, 0f,
                ref dummySpeed, FastTurnBrakeDeacceleration.Value);

            newDirection = fastTurnStartDir;

            //If finished stopping, turn to face moveDir
            if (newSpeed < FastTurnBrakeSpeedThreshold.Value)
                newDirection = moveDirection.Flatten().normalized;
        }
        
        //Cache moveInput value
        //Dont cache values in deadzone
        if (MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
            lastMoveInputDirection = MoveInput.Value.normalized;
        
        /*********************************************
         * Handle Slopes
         *********************************************/

        Vector3 newVelocity = newDirection * newSpeed;

        //If on ground that is not stable (slopes)
        if (!GroundingStatus.IsStableOnGround && GroundingStatus.FoundAnyGround)
        {
            //Take move input direction directly and flatten (Dont do turn smoothing for now)
            float slopeMoveSpeed = 10f;
            newVelocity = moveDirection.Flatten() * slopeMoveSpeed;
            
            //Project velocity sideways along slope
            slopeRight = Vector3.Cross(Vector3.up, GroundingStatus.GroundNormal);
            slopeOut = Vector3.Cross(slopeRight, Vector3.up);
            newVelocity = Vector3.ProjectOnPlane(newVelocity, slopeOut).Flatten();
        }

        return newVelocity;
    }

    private Vector3 CalculateVerticalVelocity(Vector3 currentVelocity)
    {
        CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
        CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

        Vector3 newVelocity = currentVelocity.y * Vector3.up;

        float gravity = -(2 * MaxJumpHeight.Value) / Mathf.Pow(TimeToJumpApex.Value, 2);

        if (newVelocity.y <= 0 || GroundingStatus.FoundAnyGround)  //Falling
        {
            newVelocity.y += gravity * (FallMultiplier.Value - 1) * Time.deltaTime;
        }
        else if (newVelocity.y > 0 && !JumpPressed.Value)    //Short jump
        {
            newVelocity.y -= LowJumpDrag.Value * Time.deltaTime;
            newVelocity.y += gravity * Time.deltaTime;
        }
        else
        {
            newVelocity.y += gravity * Time.deltaTime;
        }
        
        if (newVelocity.y < -Mathf.Abs(MaxFallSpeed.Value))   //Cap Speed
        {
            newVelocity.y = -Mathf.Abs(MaxFallSpeed.Value);
        }
        
        //TODO: This code lets you move upwards slope if going up but messes up the horizontal movement making it jittery
        //If just landed on slope this frame, project velocity along slope
        // if (!LastGroundingStatus.FoundAnyGround &&
        //     GroundingStatus.FoundAnyGround &&
        //     !GroundingStatus.IsStableOnGround)
        // {
        //     //newVelocity.y = 0f;
        //     newVelocity = Vector3.ProjectOnPlane(newVelocity, GroundingStatus.GroundNormal);
        // }

        //TODO: Seems like the fact that char is grounding is causing the upwards vel to 0 out
        //While on ground that is not stable (slope) project vel along slope
        if (GroundingStatus.FoundAnyGround &&
            !GroundingStatus.IsStableOnGround)
        {
            newVelocity = Vector3.ProjectOnPlane(newVelocity, GroundingStatus.GroundNormal);
        }

        return newVelocity;
    }

    private void UpdateRotation(Quaternion currentRotation)
    {
        Vector3 lookDirection = playerController.transform.forward;
        Vector3 velocityDirection = new Vector3(cachedVelocity.Value.x, 0f, cachedVelocity.Value.z);
        if (velocityDirection == Vector3.zero) velocityDirection = Vector3.forward;

        Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        Quaternion velocityRotation = Quaternion.LookRotation(velocityDirection, Vector3.up);

        if (Mathf.Approximately(velocityDirection.magnitude, 0f)) return;
        if (Mathf.Approximately(MoveInput.Value.magnitude, 0f)) return;

        currentRotation = Quaternion.RotateTowards(lookRotation, velocityRotation, RotationDeltaMax.Value);
        
        //If fast turning, instead rotate to desired turn direction
        if (isFastTurning)
        {
            Quaternion moveInputRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            currentRotation = Quaternion.RotateTowards(lookRotation, moveInputRotation, RotationDeltaMax.Value);
        }
        
        NewRotationOut.Value = currentRotation;
    }
    
    private void SetMoveDirection()
    {
        Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }

    private void CheckForFastTurn(Vector3 currentVelocity)
    {
        //Angle between move input of this frame and previous
        float deltaAngle_MoveInput = Vector3.Angle(MoveInput.Value.normalized, lastMoveInputDirection);

        //Dont fast turn if angle change is gradual (meaning they r rotating stick instead of flicking)
        //If already fast turning, dont check this
        if (!isFastTurning && deltaAngle_MoveInput < MoveInputRequiredDelta.Value)
            return;

        //If threshold is >=1 then set to infinity and disable threshold
        float FastTurnThreshold = (FastTurnPercentThreshold.Value >= 1f) ?
        Mathf.Infinity : FastTurnPercentThreshold.Value * MoveSpeed.Value;
        
        Vector2 horizontalVelocity = currentVelocity.xz();

        //Dont start fast turn if moving too fast (instead will probably brake)
        if (!isFastTurning && horizontalVelocity.magnitude > FastTurnThreshold)
            return;

        //Angle between flattened current speed and flattened desired move direction
        float deltaAngle_VelToMoveDir = Vector3.Angle(currentVelocity.Flatten().normalized, moveDirection.normalized);

        //Start fast turn if angle > ThreshHold and input magnitude > DeadZone
        if (!isFastTurning && deltaAngle_VelToMoveDir > FastTurnAngleThreshold.Value &&
            MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
        {
            isFastTurning = true;
            fastTurnStartDir = currentVelocity.Flatten().normalized;
        }
        
        //Stop fast turning when close enough to target
        else if (isFastTurning && deltaAngle_VelToMoveDir < StopFastTurnDeltaAngle.Value)
            isFastTurning = false;

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
        Vector3 endPos3 = playerController.transform.position + cachedVelocity.Value.normalized * (cachedVelocity.Value.magnitude / MoveSpeed.Value) * 10f;
        Vector3 endPos4 = playerController.transform.position + slopeRight.normalized * 5f;
        Vector3 endPos5 = playerController.transform.position + slopeOut.normalized * 5f;

        Color moveInputColor = isFastTurning ? new Color(1f, 0f, 0f, .35f) : new Color(1f, 1f, 0f, .35f);
        Color actualMoveColor = new Color(0f, 1f, 0f, .35f);
        Color cachedVelocityColor = new Color(0f, 0f, 1f, .35f);
        Color slopeRightColor = new Color(.9f, .5f, 1f, .35f);
        Color slopeUpColor = new Color(.9f, .5f, .1f, .35f);

        Draw.Line(startPos, endPos, moveInputColor);
        Draw.Line(startPos, endPos2, actualMoveColor);
        Draw.Line(startPos, endPos3, cachedVelocityColor);
        Draw.Line(startPos, endPos4, cachedVelocityColor);
        Draw.Line(startPos, endPos5, cachedVelocityColor);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos, .25f, moveInputColor);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos2, .25f, actualMoveColor);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos3, .25f, cachedVelocityColor);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos4, .25f, slopeRightColor);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos5, .25f, slopeUpColor);
    }
}
