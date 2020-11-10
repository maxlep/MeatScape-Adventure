using System.Threading;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityNavMeshAgent;
using KinematicCharacterController;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public abstract class BaseMovement : PlayerStateNode
    {
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal Movement")] [Required]
        protected FloatReference BaseSpeed;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal Movement")] [Required]
        protected FloatReference TurnSpeed;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal Movement")] [Required]
        protected FloatReference ImpulseDampingFactor;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal Movement")] [Required]
        protected FloatReference PlayerWeight;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal Movement")] [Required]
        protected TransformSceneReference PlayerCameraTransform;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
        protected Vector2Reference MoveInput;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
        protected Vector3Reference NewVelocityOut;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
        protected QuaternionReference NewRotationOut;

        protected Vector3 moveInputCameraRelative;
        protected Vector3 newDirection;
        protected float newSpeed;
        protected KinematicCharacterMotor characterMotor;

        #region Lifecycle methods
        public override void Enter()
        {
            base.Enter();
            playerController.onStartUpdateVelocity += UpdateVelocity;
            playerController.onStartUpdateRotation += UpdateRotation;
            characterMotor = playerController.CharacterMotor;
            SetMoveDirection(); //Call to force update moveDir in case updateRot called b4 updateVel
        }

        public override void Exit()
        {
            base.Exit();
            playerController.onStartUpdateVelocity -= UpdateVelocity;
            playerController.onStartUpdateRotation -= UpdateRotation;
        }
        
        public override void DrawGizmos()
        {
            base.DrawGizmos();
            
            if (playerController == null) return;
        
            // set up all static parameters. these are used for all following Draw.Line calls
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.LineThicknessSpace = ThicknessSpace.Pixels;
            Draw.LineThickness = 6; // 4px wide

            float len = 10f;
            float radius = 0.25f;
            float alpha = 0.35f;
            Vector3 startPos = playerController.transform.position;

            var lines = new (Vector3 endPos, Color color)[]
            {
                (startPos + moveInputCameraRelative * (newSpeed / BaseSpeed.Value) * len,
                    false ? new Color(1f, 0f, 0f, .35f) : new Color(1f, 1f, 0f, alpha)),
                (startPos + moveInputCameraRelative * len,
                    new Color(1f, .3f, 0.3f, alpha)),
                (startPos + newDirection * newSpeed,
                    new Color(1f, 0.5f, 0, alpha))
            };

            foreach (var line in lines)
            {
                Draw.Line(startPos, line.endPos, line.color);
                Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, line.endPos, radius, line.color);
            }
        }
        #endregion
        
        #region Update methods
        protected virtual void UpdateVelocity(Vector3 currentVelocity, Vector3 addVelocity)
        {
            SetMoveDirection();

            // Vector3 horizontalVelocity = Vector3.zero;
            // Vector3 verticalVelocity = Vector3.zero;
            //
            // horizontalVelocity = CalculateHorizontalVelocity(currentVelocity);
            // verticalVelocity = CalculateVerticalVelocity(currentVelocity);
            //
            // NewVelocityOut.Value = horizontalVelocity + verticalVelocity;

            var addVelocityDampened = addVelocity * (1 - ImpulseDampingFactor.Value);
            NewVelocityOut.Value = CalculateVelocity(currentVelocity, addVelocityDampened);
        }
        
        protected virtual void UpdateRotation(Quaternion currentRotation)
        {
            // Vector3 lookDirection = playerController.transform.forward;
            // Vector3 velocityDirection = new Vector3(cachedVelocity.Value.x, 0f, cachedVelocity.Value.z);
            // if (velocityDirection == Vector3.zero) velocityDirection = Vector3.forward;
            //
            // Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            // Quaternion velocityRotation = Quaternion.LookRotation(velocityDirection, Vector3.up);
            //
            // if (Mathf.Approximately(velocityDirection.magnitude, 0f)) return;
            // if (Mathf.Approximately(MoveInput.Value.magnitude, 0f)) return;
            //
            // currentRotation = Quaternion.RotateTowards(lookRotation, velocityRotation, RotationDeltaMax.Value);
            //
            // //If fast turning, instead rotate to desired turn direction
            // if (isFastTurning)
            // {
            //     Quaternion moveInputRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            //     currentRotation = Quaternion.RotateTowards(lookRotation, moveInputRotation, RotationDeltaMax.Value);
            // }
        
            // NewRotationOut.Value = currentRotation;

            NewRotationOut.Value = Quaternion.LookRotation(moveInputCameraRelative, Vector3.up);
        }
        #endregion
        
        private void SetMoveDirection()
        {
            Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
            moveInputCameraRelative = camForward.GetRelative(MoveInput.Value).xoy();
        }

        protected virtual Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 addVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;

            var currentDir = currentVelocity.xz();

            // var steeringDir = moveInputCameraRelative;
            // var steeringAngle = Vector3.Angle(Vector3.forward, steeringDir);
            // var steeringFac = steeringAngle / 180;

            // var aboveBaseSpeed = currentVelocity.magnitude > BaseSpeed.Value;
            //
            // var friction = PlayerWeight.Value * (1 + steeringFac);
            //
            // Vector2 dummyVel = Vector3.zero;
            // Vector2 dir = Vector2.SmoothDamp(currentVelocity.normalized, steeringDir,
            //     ref dummyVel, TurnSpeed.Value * (1 + steeringFac));
            //
            // var baseMagnitude = steeringDir.magnitude * BaseSpeed.Value;
            // var magnitude = Mathf.Max(currentVelocity.magnitude - friction, baseMagnitude);
            //
            // newDirection = steeringDir.xoy();
            // newSpeed = magnitude;
            //
            // return newDirection * newSpeed;

            // newDirection = Vector3.forward;
            // newSpeed = BaseSpeed.Value;
            // return newDirection * newSpeed;
            return Vector3.zero;
        }
        //
        // private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
        // {
        //     CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
        //
        //     /**************************************
        //      * Get New Move Direction
        //      **************************************/
        //
        //     //Rotate current vel towards target vel to get new direction
        //     Vector2 dummyVel = Vector3.zero;
        //     Vector2 dir = Vector2.SmoothDamp(horizontalVelocity.normalized, moveDirection.xz(),
        //         ref dummyVel, currentTurnSpeed);
        //
        //     newDirection = dir.xoy().normalized;
        //
        //     /********************************************
        //      * Get New Move Direction (Project On Slope)
        //      ********************************************/
        //     if (ProjectOnGroundPlane)
        //     {
        //         slopeRight = Vector3.Cross(Vector3.down, GroundingStatus.GroundNormal).normalized;
        //
        //         //Signed angle between slope right and camera right (to get camera and slope relative)
        //         float slopeRightToCamRight = Vector3.SignedAngle(slopeRight,
        //             PlayerCameraTransform.Value.right.xoz(), Vector3.up);
        //
        //         //Get angle of moveInput on Unit Circle (Degrees from right position)
        //         float moveInputAngle = MoveInput.Value.AngleOnUnitCircle();
        //
        //         //Combine input and slope/relative camera angle
        //         float camSlopeRelativeMoveAngle = slopeRightToCamRight - moveInputAngle;
        //
        //         //Rotate the slope right around the slope normal by camSlopeRelativeMoveAngle
        //         //This essentially projects the XZ-bound camSlopeRelativeMoveAngle downwards onto the slope
        //         projectedDirection = (Quaternion.AngleAxis(camSlopeRelativeMoveAngle, GroundingStatus.GroundNormal)
        //                               * slopeRight).normalized;
        //
        //         newDirection = projectedDirection;
        //     }
        //
        //     /**************************************
        //      * Get New Move Speed
        //      **************************************/
        //
        //     //Accelerate/DeAccelerate from current Speed to target speed
        //     float dummySpeed = 0f;
        //     float currentSpeed;
        //     float targetSpeed;
        //
        //     if (ProjectOnGroundPlane)
        //     {
        //         currentSpeed = currentVelocity.magnitude;
        //         targetSpeed = MoveInput.Value.magnitude * Mathf.Lerp(MoveSpeed.Value, SlopeSlowMoveSpeed.Value,
        //             SlopeSlideTimer.ElapsedTime / SlopeSlideTimer.Duration);
        //     }
        //     else
        //     {
        //         currentSpeed = currentVelocity.xoz().magnitude;
        //         targetSpeed = MoveInput.Value.magnitude * MoveSpeed.Value;
        //     }
        //
        //
        //     if (targetSpeed > currentSpeed)
        //         newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
        //             ref dummySpeed, Acceleration.Value);
        //     else
        //         newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
        //             ref dummySpeed, Deacceleration.Value);
        //
        //     /*********************************************
        //      * Override Speed and Direction if Fast Turning
        //      *********************************************/
        //
        //     //If fast turning, DeAccelerate to 0 to brake
        //     if (isFastTurning)
        //     {
        //         newSpeed = Mathf.SmoothDamp(currentSpeed, 0f,
        //             ref dummySpeed, FastTurnBrakeDeacceleration.Value);
        //
        //         newDirection = fastTurnStartDir;
        //
        //         //If finished stopping, turn to face moveDir
        //         if (newSpeed < FastTurnBrakeSpeedThreshold.Value)
        //             newDirection = moveDirection.xoz().normalized;
        //     }
        //
        //     //Cache moveInput value
        //     //Dont cache values in deadzone
        //     if (MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
        //         lastMoveInputDirection = MoveInput.Value.normalized;
        //
        //
        //     return newDirection * newSpeed;
        //     ;
        // }

        // private Vector3 CalculateVerticalVelocity(Vector3 currentVelocity)
        // {
        //     CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
        //     CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;
        //
        //     Vector3 newVelocity = currentVelocity.y * Vector3.up;
        //
        //     float gravity = -(2 * MaxJumpHeight.Value) / Mathf.Pow(TimeToJumpApex.Value, 2);
        //
        //     if (newVelocity.y <= 0 || GroundingStatus.FoundAnyGround) //Falling
        //     {
        //         newVelocity.y += gravity * (FallMultiplier.Value - 1) * Time.deltaTime;
        //     }
        //     else if (newVelocity.y > 0 && !JumpPressed.Value) //Short jump
        //     {
        //         newVelocity.y -= LowJumpDrag.Value * Time.deltaTime;
        //         newVelocity.y += gravity * Time.deltaTime;
        //     }
        //     else
        //     {
        //         newVelocity.y += gravity * Time.deltaTime;
        //     }
        //
        //     if (newVelocity.y < -Mathf.Abs(MaxFallSpeed.Value)) //Cap Speed
        //     {
        //         newVelocity.y = -Mathf.Abs(MaxFallSpeed.Value);
        //     }
        //
        //     return newVelocity;
        // }

        
    }
}