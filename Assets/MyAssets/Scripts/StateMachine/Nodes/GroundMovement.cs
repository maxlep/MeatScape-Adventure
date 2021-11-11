using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using KinematicCharacterController;
using MyAssets.Graphs.StateMachine.Nodes;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class GroundMovement : BaseMovement
    {
        #region Horizontal Movement

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatValueReference MoveSpeed;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatReference RotationDeltaMax;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatReference Acceleration;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatReference Deacceleration;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private Vector3Reference cachedVelocity;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private CurveReference SlopeSlowdownCurve;

        #endregion
        
        #region Grounding

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Grounding")] [Required]
        private FloatReference GroundStickAngleInput;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Grounding")] [Required]
        private FloatReference GroundStickAngleOutput;

        #endregion

        #region Fast Turn

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [Required]
        private bool EnableFastTurn = true;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnSpeed;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnPercentThreshold;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnAngleThreshold;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference StopFastTurnDeltaAngle;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnInputDeadZone;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnBrakeDeacceleration;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference SlideTurnBrakeDeacceleration;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnBrakeSpeedThreshold;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference MoveInputRequiredDelta;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")] [Required]
        private BoolReference IsFastTurning;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")] [Required]
        private BoolReference IsSlideTurning;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")] [Required]
        private FloatValueReference SlideTurnThresholdSpeed;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")] [Required]
        private FloatValueReference SlideTurnThresholdSpeedFactor;


        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")] [Required]
        private TriggerVariable FastTurnTriggered;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")] [Required]
        private TriggerVariable SlideTurnTriggered;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")] [Required]
        private TriggerVariable FastTurnCompleted;

        #endregion

        private Vector3 fastTurnStartDir;
        private Vector3 lastMoveInputDirection = Vector3.zero;
        private Vector3 slopeOut;
        private Vector3 intersectingVector;

        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();

            GroundStickAngleOutput.Value = GroundStickAngleInput.Value;
        }

        public override void Exit()
        {
            base.Exit();
            IsFastTurning.Value = false;
            IsSlideTurning.Value = false;
        }

        #endregion

        protected override Vector3 CalculateVelocity(VelocityInfo velocityInfo)
        {
            Vector3 currentVelocity = velocityInfo.currentVelocity;
            Vector3 impulseVelocity = velocityInfo.impulseVelocity;
            Vector3 impulseVelocityRedirectble = velocityInfo.impulseVelocityRedirectble;
            
            //Kill Y velocity if just get grounded so not to slide
            if (characterMotor.GroundingStatus.IsStableOnGround && !characterMotor.LastGroundingStatus.IsStableOnGround)
            {
                currentVelocity = Vector3.ProjectOnPlane(currentVelocity , characterMotor.CharacterUp);
                currentVelocity  = characterMotor.GetDirectionTangentToSurface(currentVelocity ,
                    characterMotor.GroundingStatus.GroundNormal) * currentVelocity .magnitude;
            }
            
            float currentVelocityMagnitude = currentVelocity.magnitude;
            KinematicCharacterMotor motor = playerController.CharacterMotor;
            
            #region Effective Normal & Reorient Vel on Slope
            
            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;
            
            if (motor.GroundingStatus.FoundAnyGround)
            {
                //Get effective ground normal based on move direction
                effectiveGroundNormal = CalculateEffectiveGroundNormal(currentVelocity, currentVelocityMagnitude, motor);

                // Reorient velocity on slope
                currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
            }
            
            #endregion

            Vector3 horizontalVelocity = CalculateHorizontalVelocity(currentVelocity, effectiveGroundNormal);
            
            //Addive in XZ but sets in Y
            return horizontalVelocity + impulseVelocity;
        }

        private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity, Vector3 effectiveGroundNormal)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            //Vector2 horizontalVelocity = currentVelocity.xz();
            float currentTurnSpeed;
            float currentSpeed;

            #region Determine if Fast Turning

            //Dont allow fast turn on slopes
            if (EnableFastTurn)
                CheckForFastTurn(currentVelocity);

            //Update turn speed based on isFastTurning
            if (IsFastTurning.Value)
                currentTurnSpeed = FastTurnSpeed.Value;
            else
                currentTurnSpeed = TurnSpeed.Value;

            #endregion

            #region Get New Move Direction

            //Rotate current vel towards target vel to get new direction
            Vector3 dummyVel = Vector3.zero;
            Vector3 targetDir = VectorUtils.FlattenDirectionOntoSlope(moveInputCameraRelative.xoz(), effectiveGroundNormal);
            Vector3 dir = Vector3.SmoothDamp(currentVelocity.normalized, targetDir,
                ref dummyVel, currentTurnSpeed);

            newDirection = dir.normalized;
            
            #endregion
            
            #region Slope Slowdown

            float moveAngleAboveHorizontal = Vector3.SignedAngle(newDirection, newDirection.xoz(),
                Vector3.Cross(newDirection, Vector3.down));
            float slownessFactor = SlopeSlowdownCurve.Value.Evaluate(moveAngleAboveHorizontal);
            float slopeSlowedMoveSpeed = slownessFactor * MoveSpeed.Value;

            #endregion

            #region Get New Move Speed

            //Accelerate/DeAccelerate from current Speed to target speed
            float dummySpeed = 0f;
            float targetSpeed;

            currentSpeed = currentVelocity.magnitude;
            targetSpeed = MoveInput.Value.magnitude * slopeSlowedMoveSpeed;
            
            if (targetSpeed > currentSpeed)
                newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
                    ref dummySpeed, Acceleration.Value);
            else
                newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
                    ref dummySpeed, Deacceleration.Value);
            
            #endregion

            #region Override Speed and Direction if Fast Turning
            
            dummySpeed = 0f;

            //If fast turning, DeAccelerate to 0 to brake
            if (IsFastTurning.Value)
            {
                newSpeed = Mathf.SmoothDamp(currentSpeed, 0f,
                    ref dummySpeed, FastTurnBrakeDeacceleration.Value);

                newDirection = fastTurnStartDir;

                //If finished stopping, turn to face moveDir
                if (newSpeed < FastTurnBrakeSpeedThreshold.Value)
                    newDirection = VectorUtils.FlattenDirectionOntoSlope(moveInputCameraRelative.xoz(), effectiveGroundNormal);
            }
            else if(IsSlideTurning.Value)
            {
                newSpeed = Mathf.SmoothDamp(currentSpeed, 0f,
                    ref dummySpeed, SlideTurnBrakeDeacceleration.Value);

                newDirection = fastTurnStartDir;

                //If finished stopping, turn to face moveDir
                if (newSpeed < FastTurnBrakeSpeedThreshold.Value)
                    newDirection = VectorUtils.FlattenDirectionOntoSlope(moveInputCameraRelative.xoz(), effectiveGroundNormal);
            }

            #endregion
            
            //Cache moveInput value
            //Dont cache values in deadzone
            if (MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
                lastMoveInputDirection = MoveInput.Value.normalized;
            

            return newDirection * newSpeed;
        }
        
        protected override void UpdateRotation(Quaternion currentRotation)
        {
            Quaternion newRotation;
            Vector3 lookDirection = playerController.transform.forward;
            //Vector3 velocityDirection = cachedVelocity.Value.xoz().normalized;
            Vector3 velocityDirection = moveInputCameraRelative.normalized;
            if (Mathf.Approximately(velocityDirection.magnitude, 0f)) velocityDirection = lookDirection;

            Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            Quaternion velocityRotation = Quaternion.LookRotation(velocityDirection, Vector3.up);

            newRotation = Quaternion.RotateTowards(lookRotation, velocityRotation, RotationDeltaMax.Value);
        
            //If fast turning, instead rotate to desired turn direction
            if (IsFastTurning.Value)
            {
                Quaternion moveInputRotation = Quaternion.LookRotation(moveInputCameraRelative.xoz(), Vector3.up);
                newRotation = Quaternion.RotateTowards(lookRotation, moveInputRotation, RotationDeltaMax.Value);
            }
        
            NewRotationOut.Value = newRotation;
        }
        
        private void CheckForFastTurn(Vector3 currentVelocity)
        {
            //Angle between move input of this frame and previous
            float deltaAngle_MoveInput = Vector3.Angle(MoveInput.Value.normalized, lastMoveInputDirection);
            bool noTurn = (!IsFastTurning.Value && !IsSlideTurning.Value);

            //Dont fast turn if angle change is gradual (meaning they r rotating stick instead of flicking)
            //If already fast turning, dont check this
            if (noTurn && deltaAngle_MoveInput < MoveInputRequiredDelta.Value)
                return;

            //If threshold is >=1 then set to infinity and disable threshold
            float FastTurnThreshold = (FastTurnPercentThreshold.Value >= 1f) ?
                Mathf.Infinity : FastTurnPercentThreshold.Value * MoveSpeed.Value;
        
            float currentSpeed = currentVelocity.magnitude;

            //Dont start fast turn if moving too fast (instead will probably brake)
            if ((noTurn) && currentSpeed > FastTurnThreshold)
                return;

            //Angle between flattened current speed and flattened desired move direction
            float deltaAngle_VelToMoveDir = Vector3.Angle(currentVelocity.xoz().normalized, moveInputCameraRelative.normalized);

            //Start fast turn if angle > ThreshHold and input magnitude > DeadZone
            if (noTurn && deltaAngle_VelToMoveDir > FastTurnAngleThreshold.Value &&
                MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
            {
                if (currentSpeed >= SlideTurnThresholdSpeed.Value * SlideTurnThresholdSpeedFactor.Value)
                {
                    IsSlideTurning.Value = true;
                    SlideTurnTriggered.Activate();
                }
                else
                {
                    IsFastTurning.Value = true;
                    FastTurnTriggered.Activate();
                }

                fastTurnStartDir = currentVelocity.xoz().normalized;
                
            }
        
            //Stop fast turning when close enough to target
            else if ((IsFastTurning.Value || IsSlideTurning.Value) && deltaAngle_VelToMoveDir < StopFastTurnDeltaAngle.Value)
            {
                IsFastTurning.Value = false;
                IsSlideTurning.Value = false;
                FastTurnCompleted.Activate();
            }
        }

        public override void DrawGizmos()
        {
            if (playerController == null) return;
        
            // set up all static parameters. these are used for all following Draw.Line calls
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.LineThicknessSpace = ThicknessSpace.Meters;
            Draw.LineThickness = .1f;

            Vector3 startPos = playerController.transform.position;
            Vector3 endPos = playerController.transform.position + newDirection * (newSpeed / MoveSpeed.Value) * 10f;
            Vector3 endPos2 = playerController.transform.position + moveInputCameraRelative * 10f;
            Vector3 endPos4 = playerController.transform.position + cachedVelocity.Value.normalized * (cachedVelocity.Value.magnitude / MoveSpeed.Value) * 10f;
            Vector3 endPos6 = playerController.transform.position + playerController.GroundingStatus.GroundNormal * 10f;
            Vector3 endPos7 = playerController.transform.position + intersectingVector * 10f;

            Color actualMoveColor = IsFastTurning.Value ? new Color(1f, 0f, 0f, .35f) : new Color(1f, 1f, 0f, .35f);
            Color moveInputColor = new Color(0f, 1f, 0f, .35f);
            Color projectedMoveInputColor = new Color(.3f, 1f, .8f, .35f);
            Color cachedVelocityColor = new Color(0f, 0f, 1f, .35f);
            Color slopeRightColor = Color.magenta;
            Color groundNormalColor = Color.white;

            Draw.Line(startPos, endPos, actualMoveColor); //Actual move
            Draw.Line(startPos, endPos2, moveInputColor); //Move Input
            Draw.Line(startPos, endPos4, cachedVelocityColor); //CachedVelocity
            //Draw.Line(startPos, endPos5, slopeRightColor); //Slope Right
            //Draw.Line(startPos, endPos6, groundNormalColor); //Ground Normal
            Draw.Line(startPos, endPos7, slopeRightColor); //Intersecting Vector
            
            
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos, .25f, actualMoveColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos2, .25f, moveInputColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos4, .25f, cachedVelocityColor);
            //Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos5, .25f, slopeRightColor);
            //Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos6, .25f, groundNormalColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos7, .25f, slopeRightColor);
        }
    }
}
