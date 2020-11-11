using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using MyAssets.Graphs.StateMachine.Nodes;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class GroundMovement : BaseMovement
    {
        #region Horizontal Movement

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal Movement")] [Required]
        private FloatReference MoveSpeed;

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
        private Vector3Reference cachedVelocity;

        #endregion

        #region Fast Turn
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [Required]
        private bool EnableFastTurn = true;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnSpeed;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference FastTurnPercentThreshold;

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
        private FloatReference FastTurnBrakeSpeedThreshold;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference MoveInputRequiredDelta;

        #endregion

        private bool isFastTurning;
        private Vector3 fastTurnStartDir;
        private Vector3 lastMoveInputDirection = Vector3.zero;
        private Vector3 slopeOut;

        protected override Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 addImpulse)
        {
            Vector3 horizontalVelocity = CalculateHorizontalVelocity(currentVelocity);

            //Addive in XZ but sets in Y
            return horizontalVelocity + addImpulse;
        }

        private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;

            #region Determine if Fast Turning

            //Dont allow fast turn on slopes
            if (EnableFastTurn)
                CheckForFastTurn(currentVelocity);
            
            float currentTurnSpeed;
            Vector2 horizontalVelocity = currentVelocity.xz();

            //Update turn speed based on isFastTurning
            if (isFastTurning)
                currentTurnSpeed = FastTurnSpeed.Value;
            else
                currentTurnSpeed = TurnSpeed.Value;

            #endregion

            #region Get New Move Direction

            //Rotate current vel towards target vel to get new direction
            Vector2 dummyVel = Vector3.zero;
            Vector2 dir = Vector2.SmoothDamp(horizontalVelocity.normalized, moveInputCameraRelative.xz(),
                ref dummyVel, currentTurnSpeed);
            
            newDirection = dir.xoy().normalized;

            #endregion

            #region Get New Move Speed

            //Accelerate/DeAccelerate from current Speed to target speed
            float dummySpeed = 0f;
            float currentSpeed;
            float targetSpeed;

            currentSpeed = currentVelocity.xoz().magnitude;
            targetSpeed = MoveInput.Value.magnitude * MoveSpeed.Value;
            
            if (targetSpeed > currentSpeed)
                newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
                    ref dummySpeed, Acceleration.Value);
            else
                newSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed,
                    ref dummySpeed, Deacceleration.Value);

            #endregion

            #region Override Speed and Direction if Fast Turning

            //If fast turning, DeAccelerate to 0 to brake
            if (isFastTurning)
            {
                newSpeed = Mathf.SmoothDamp(currentSpeed, 0f,
                    ref dummySpeed, FastTurnBrakeDeacceleration.Value);

                newDirection = fastTurnStartDir;

                //If finished stopping, turn to face moveDir
                if (newSpeed < FastTurnBrakeSpeedThreshold.Value)
                    newDirection = moveInputCameraRelative.xoz().normalized;
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
            if (isFastTurning)
            {
                Quaternion moveInputRotation = Quaternion.LookRotation(moveInputCameraRelative, Vector3.up);
                newRotation = Quaternion.RotateTowards(lookRotation, moveInputRotation, RotationDeltaMax.Value);
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

            //If threshold is >=1 then set to infinity and disable threshold
            float FastTurnThreshold = (FastTurnPercentThreshold.Value >= 1f) ?
                Mathf.Infinity : FastTurnPercentThreshold.Value * MoveSpeed.Value;
        
            Vector2 horizontalVelocity = currentVelocity.xz();

            //Dont start fast turn if moving too fast (instead will probably brake)
            if (!isFastTurning && horizontalVelocity.magnitude > FastTurnThreshold)
                return;

            //Angle between flattened current speed and flattened desired move direction
            float deltaAngle_VelToMoveDir = Vector3.Angle(currentVelocity.xoz().normalized, moveInputCameraRelative.normalized);

            //Start fast turn if angle > ThreshHold and input magnitude > DeadZone
            if (!isFastTurning && deltaAngle_VelToMoveDir > FastTurnAngleThreshold.Value &&
                MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
            {
                isFastTurning = true;
                fastTurnStartDir = currentVelocity.xoz().normalized;
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
            Draw.LineThicknessSpace = ThicknessSpace.Meters;
            Draw.LineThickness = .1f;

            Vector3 startPos = playerController.transform.position;
            Vector3 endPos = playerController.transform.position + newDirection * (newSpeed / MoveSpeed.Value) * 10f;
            Vector3 endPos2 = playerController.transform.position + moveInputCameraRelative * 10f;
            Vector3 endPos4 = playerController.transform.position + cachedVelocity.Value.normalized * (cachedVelocity.Value.magnitude / MoveSpeed.Value) * 10f;
            Vector3 endPos6 = playerController.transform.position + playerController.GroundingStatus.GroundNormal * 10f;

            Color actualMoveColor = isFastTurning ? new Color(1f, 0f, 0f, .35f) : new Color(1f, 1f, 0f, .35f);
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
            
            
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos, .25f, actualMoveColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos2, .25f, moveInputColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos4, .25f, cachedVelocityColor);
            //Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos5, .25f, slopeRightColor);
            //Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos6, .25f, groundNormalColor);
        }
    }
}
