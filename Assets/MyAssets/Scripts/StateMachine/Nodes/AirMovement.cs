using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using MyAssets.Graphs.StateMachine.Nodes;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Jobs;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class AirMovement : BaseMovement
    {

        #region Horizontal Movement

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatValueReference MoveSpeed;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatReference RotationDeltaMax;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatReference Acceleration;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatReference AccelerationSlow;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatReference Deacceleration;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private FloatReference FaceForwardDeltaDegrees;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal")] [Required]
        private Vector3Reference cachedVelocity;

        #endregion

        #region Vertical Movement

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical")]
        private FloatReference FallMultiplier;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical")]
        private FloatReference LowJumpDrag;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical")]
        private FloatReference MaxFallSpeed;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical")]
        private BoolReference JumpPressed;

        #endregion

        #region Fast Turn
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [Required]
        private bool EnableFastTurn = true;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnDeltaRot;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnAcceleration;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnAngleThreshold;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference StopFastTurnDeltaAngle;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnInputDeadZone;
    
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnBrakeDeacceleration;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnMinBrakeDrag;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnMinBrakeSpeed;
    
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnBrakeSpeedThreshold;
    
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference MoveInputRequiredDelta;

        #endregion

        private bool isFastTurning;
        private bool isFastTurningRotation;
        private Vector3 fastTurnStartDir;
        private Vector3 fastTurnStartMoveDir;
        private Vector3 lastMoveInputDirection = Vector3.zero;

        public override void Exit()
        {
            base.Exit();
            isFastTurning = false;
            isFastTurningRotation = false;
        }

        protected override Vector3 CalculateVelocity(VelocityInfo velocityInfo)
        {
            Vector3 currentVelocity = velocityInfo.currentVelocity;
            Vector3 impulseVelocity = velocityInfo.impulseVelocity;
            Vector3 impulseVelocityRedirectble = velocityInfo.impulseVelocityRedirectble;

            Vector3 totalImpulse = impulseVelocity;
            Vector3 resultingVelocity;
            Vector3 horizontalVelocity = CalculateHorizontalVelocity(currentVelocity);
            Vector3 verticalVelocity = CalculateVerticalVelocity(currentVelocity);

            //Redirect impulseVelocityRedirectble if conditions met
            if (EnableRedirect && CheckRedirectConditions(impulseVelocityRedirectble))
                totalImpulse += CalculateRedirectedImpulse(impulseVelocityRedirectble);
            else
                totalImpulse += impulseVelocityRedirectble;

            //Cache moveInput value for fast turn
            if (MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
                lastMoveInputDirection = MoveInput.Value.normalized;
            
            if (Mathf.Approximately(totalImpulse.magnitude, 0f))
                resultingVelocity = horizontalVelocity + verticalVelocity;
            
            //If y of impulseVelocity is 0, dont override current y component
            else if (Mathf.Approximately(totalImpulse.y, 0f))
                resultingVelocity = horizontalVelocity + verticalVelocity + totalImpulse;
            
            //Add impulse and override current y component
            else
                resultingVelocity = horizontalVelocity + totalImpulse;

            return resultingVelocity;

        }

        private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
        {
            if (EnableFastTurn)
                CheckForFastTurn(currentVelocity);

            #region Calulcate Drag

            float dragCoefficient = (isFastTurning) ? FastTurnBrakeDeacceleration.Value : Deacceleration.Value;
            float dragMagnitude = currentVelocity.xoz().sqrMagnitude * dragCoefficient * Time.deltaTime;
            
            //If fast turning, use min drag
            if (isFastTurning) 
                dragMagnitude = Mathf.Max(dragMagnitude, FastTurnMinBrakeDrag.Value);
            
            Vector3 dragVelocity = -currentVelocity.xoz().normalized * dragMagnitude;

            #endregion

            #region Calulcate Acceleration
            
            float acceleration = Acceleration.Value;
            float forwardToMoveInputDegrees = Vector3.Angle(playerController.transform.forward,
                moveInputCameraRelative.normalized);
            
            //Slow acceleration if moving sideways and !fastTurning
            if (forwardToMoveInputDegrees > FaceForwardDeltaDegrees.Value)
                acceleration = AccelerationSlow.Value;
            if (isFastTurning)
                acceleration = FastTurnAcceleration.Value;
            
            #endregion
            
            //If fast turning, move towards the fast turn start move direction
            if (isFastTurning)
                return currentVelocity.xoz() + fastTurnStartMoveDir * acceleration + dragVelocity;

            return currentVelocity.xoz() + moveInputCameraRelative * acceleration + dragVelocity;
        }
        
        private Vector3 CalculateVerticalVelocity(Vector3 currentVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

            Vector3 newVelocity = currentVelocity.y * Vector3.up;
            
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

            return newVelocity;
        }
        
        protected override void UpdateRotation(Quaternion currentRotation)
        {
            Quaternion newRotation;
            Quaternion lookRotation = Quaternion.LookRotation(playerController.transform.forward, Vector3.up);
            
            float deltaRotation = (isFastTurning) ? FastTurnDeltaRot.Value : RotationDeltaMax.Value;
            
            //Start rotation fast turn to flip to target
            if (isFastTurning && !isFastTurningRotation)
                isFastTurningRotation = true;

            if (isFastTurningRotation)
            {
                Quaternion fastTurnStartDirRotation = Quaternion.LookRotation(fastTurnStartMoveDir, Vector3.up);
                newRotation = Quaternion.RotateTowards(lookRotation, fastTurnStartDirRotation, deltaRotation);
            }
            //If no move input, keep looking forward
            else if (Mathf.Approximately(moveInputCameraRelative.magnitude, 0f))
            {
                newRotation = lookRotation;
            }
            else
            {
                Quaternion moveInputRotation = Quaternion.LookRotation(moveInputCameraRelative.normalized,
                    Vector3.up);
                newRotation = Quaternion.RotateTowards(lookRotation, moveInputRotation, deltaRotation);
            }

            float degreesToFastTurnTarget = Vector3.Angle(playerController.transform.forward, fastTurnStartMoveDir);
            
            //Handle finishing Rotation Fast Turn
            if (isFastTurningRotation && degreesToFastTurnTarget < StopFastTurnDeltaAngle.Value)
            {
                isFastTurningRotation = false;
            }

            NewRotationOut.Value = newRotation;
        }
        
        private void CheckForFastTurn(Vector3 currentVelocity)
        {
            //Angle between move input of this frame and previous
            float deltaAngle_MoveInput = Vector3.Angle(MoveInput.Value.normalized, lastMoveInputDirection);

            //Dont fast turn if angle change is gradual (meaning they r rotating stick instead of flicking)
            //If already fast turning, dont check this
            if (!isFastTurning && deltaAngle_MoveInput < MoveInputRequiredDelta.Value)
                return;

            float deltaAngle_VelToMoveDir;
            
            //Angle between move input and velocity
            //Or forward and velocity if move input approx. 0
            if (Mathf.Approximately(currentVelocity.xoz().magnitude, 0f))
                deltaAngle_VelToMoveDir = Vector3.Angle(currentVelocity.xoz().normalized, 
                    moveInputCameraRelative.normalized);
            else
                deltaAngle_VelToMoveDir = Vector3.Angle(playerController.transform.forward, 
                    moveInputCameraRelative.normalized);
            
            float deltaAngle_VelToStartMoveDir = Vector3.Angle(currentVelocity.xoz().normalized, 
                fastTurnStartMoveDir);
        
            //Stop fast turning when close enough to target
            //Or going slow enough
            if (isFastTurning && deltaAngle_VelToStartMoveDir < StopFastTurnDeltaAngle.Value ||
                currentVelocity.xoz().magnitude < FastTurnMinBrakeSpeed.Value)
                isFastTurning = false;

            //Start fast turn if angle > ThreshHold and input magnitude > DeadZone
            if (!isFastTurning && deltaAngle_VelToMoveDir > FastTurnAngleThreshold.Value &&
                MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
            {
                isFastTurning = true;
                fastTurnStartDir = currentVelocity.xoz().normalized;
                fastTurnStartMoveDir = moveInputCameraRelative.normalized;
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
            Vector3 endPos = playerController.transform.position + NewVelocityOut.Value.xoz().normalized * 10f;
            Vector3 endPos2 = playerController.transform.position + moveInputCameraRelative * 10f;
            Vector3 endPos4 = playerController.transform.position + cachedVelocity.Value.normalized * (cachedVelocity.Value.magnitude / MoveSpeed.Value) * 10f;
            Vector3 endPos5 = playerController.transform.position + fastTurnStartMoveDir * 10f;

            Color actualMoveColor = isFastTurning ? new Color(1f, 0f, 0f, .35f) : new Color(1f, 1f, 0f, .35f);
            Color moveInputColor = new Color(0f, 1f, 0f, .35f);
            Color fastTurnStartMoveDirColor = new Color(.3f, 1f, .8f, .35f);
            Color cachedVelocityColor = new Color(0f, 0f, 1f, .35f);
            
            Draw.Line(startPos, endPos, actualMoveColor); //Actual move
            Draw.Line(startPos, endPos2, moveInputColor); //Move Input
            Draw.Line(startPos, endPos4, cachedVelocityColor); //CachedVelocity
            Draw.Line(startPos, endPos5, fastTurnStartMoveDirColor); //FastTurnStartMoveDir

            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos, .25f, actualMoveColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos2, .25f, moveInputColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos4, .25f, cachedVelocityColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos5, .25f, fastTurnStartMoveDirColor);
        }
    }
}
