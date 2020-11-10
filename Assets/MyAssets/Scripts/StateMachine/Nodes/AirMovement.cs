using System.Collections;
using System.Collections.Generic;
using AmplifyShaderEditor;
using KinematicCharacterController;
using MyAssets.Graphs.StateMachine.Nodes;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class AirMovement : BaseMovement
    {
        /**************************************
        * Horizontal Movement *
        **************************************/

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal Movement")] [Required]
        private bool EnableFastTurn = true;

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
        private FloatReference AccelerationSlow;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal Movement")] [Required]
        private FloatReference Deacceleration;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal Movement")] [Required]
        private FloatReference FaceForwardDeltaDegrees;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Horizontal Movement")] [Required]
        private Vector3Reference cachedVelocity;

        /**************************************
            * Vertical Movement *
        **************************************/

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")] 
        private FloatReference TimeToJumpApex;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")] 
        private FloatReference MaxJumpHeight;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")]
        private FloatReference FallMultiplier;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")]
        private FloatReference LowJumpDrag;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")]
        private FloatReference MaxFallSpeed;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")]
        private BoolReference JumpPressed;
        
        /**************************************
             * Fast Turn *
    **************************************/
    
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
        private FloatReference FastTurnBrakeSpeedThreshold;
    
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Fast Turn")] [ShowIf("$EnableFastTurn")]
        [Required]
        private FloatReference MoveInputRequiredDelta;
        
        private bool isFastTurning;
        private Vector3 fastTurnStartDir;
        private Vector3 lastMoveInputDirection = Vector3.zero;

        protected override Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 addVelocity)
        {
            Vector3 horizontalVelocity = Vector3.zero;
            Vector3 verticalVelocity = Vector3.zero;
            
            horizontalVelocity = CalculateHorizontalVelocity(currentVelocity);
            verticalVelocity = CalculateVerticalVelocity(currentVelocity);
            
            var maxRedirectDegrees = 45f;
            var reditectDegrees = 45f * moveInputCameraRelative.magnitude; //Make it range based on move input
            var addVelocityRedirectedDir = Vector3.RotateTowards(addVelocity.normalized,
                moveInputCameraRelative.normalized, reditectDegrees * Mathf.Deg2Rad, 0f);
            var addVelocityRedirected = addVelocity.magnitude * addVelocityRedirectedDir;
            
            //Cache moveInput value
            //Dont cache values in deadzone
            if (MoveInput.Value.magnitude > FastTurnInputDeadZone.Value)
                lastMoveInputDirection = MoveInput.Value.normalized;

            if (Mathf.Approximately(addVelocity.magnitude, 0f))
            {
                return horizontalVelocity + verticalVelocity;
            }
            else
            {
                //Override vertical velocity
                return horizontalVelocity + addVelocity.oyo() + addVelocityRedirected.xoz();
            }
            
           
        }

        private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
        {
            if (EnableFastTurn)
                CheckForFastTurn(currentVelocity);
            
            float deacceleration = (isFastTurning) ? FastTurnBrakeDeacceleration.Value : Deacceleration.Value;

            
            Vector3 dragVelocity = -currentVelocity.xoz().normalized * currentVelocity.xoz().sqrMagnitude *
                                   deacceleration * Time.deltaTime;

            float acceleration = Acceleration.Value;
            float forwardToMoveInputDegrees = Vector3.Angle(playerController.transform.forward,
                moveInputCameraRelative.normalized);
            
            //Slow acceleration if moving sideways and !fastTurning
            if (forwardToMoveInputDegrees > FaceForwardDeltaDegrees.Value)
                acceleration = AccelerationSlow.Value;
            if (isFastTurning)
                acceleration = FastTurnAcceleration.Value;

            return currentVelocity.xoz() + moveInputCameraRelative * acceleration + dragVelocity;
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

            return newVelocity;
        }
        
        protected override void UpdateRotation(Quaternion currentRotation)
        {
            Vector3 lookDirection = playerController.transform.forward;
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            Quaternion moveInputRotation = Quaternion.LookRotation(moveInputCameraRelative.normalized,
                Vector3.up);

            if (Mathf.Approximately(moveInputCameraRelative.magnitude, 0f)) return;

            float deltaRotation = (isFastTurning) ? FastTurnDeltaRot.Value : RotationDeltaMax.Value;

            currentRotation = Quaternion.RotateTowards(lookRotation, moveInputRotation, deltaRotation);

            NewRotationOut.Value = currentRotation;
        }
        
        private void CheckForFastTurn(Vector3 currentVelocity)
        {
            //Angle between move input of this frame and previous
            float deltaAngle_MoveInput = Vector3.Angle(MoveInput.Value.normalized, lastMoveInputDirection);

            //Dont fast turn if angle change is gradual (meaning they r rotating stick instead of flicking)
            //If already fast turning, dont check this
            if (!isFastTurning && deltaAngle_MoveInput < MoveInputRequiredDelta.Value)
                return;

            //Angle between flattened current speed and flattened desired move direction
            float deltaAngle_VelToMoveDir = Vector3.Angle(currentVelocity.xoz().normalized, 
                moveInputCameraRelative.normalized);

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
            Vector3 endPos = playerController.transform.position + NewVelocityOut.Value.normalized * 10f;
            Vector3 endPos2 = playerController.transform.position + moveInputCameraRelative * 10f;
            Vector3 endPos4 = playerController.transform.position + cachedVelocity.Value.normalized * (cachedVelocity.Value.magnitude / MoveSpeed.Value) * 10f;
            Vector3 endPos6 = playerController.transform.position + playerController.GroundingStatus.GroundNormal * 10f;

            Color actualMoveColor = isFastTurning ? new Color(1f, 0f, 0f, .35f) : new Color(1f, 1f, 0f, .35f);
            Color moveInputColor = new Color(0f, 1f, 0f, .35f);
            Color projectedMoveInputColor = new Color(.3f, 1f, .8f, .35f);
            Color cachedVelocityColor = new Color(0f, 0f, 1f, .35f);
            
            Draw.Line(startPos, endPos, actualMoveColor); //Actual move
            Draw.Line(startPos, endPos2, moveInputColor); //Move Input
            Draw.Line(startPos, endPos4, cachedVelocityColor); //CachedVelocity

            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos, .25f, actualMoveColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos2, .25f, moveInputColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos4, .25f, cachedVelocityColor);
        }
    }
}
