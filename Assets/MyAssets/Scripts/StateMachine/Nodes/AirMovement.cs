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
        private FloatReference Deacceleration;

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
        
        protected override Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 addVelocity)
        {
            Vector3 horizontalVelocity = Vector3.zero;
            Vector3 verticalVelocity = Vector3.zero;


            horizontalVelocity = CalculateHorizontalVelocity(currentVelocity);
            verticalVelocity = CalculateVerticalVelocity(currentVelocity);

            //Addive in XZ but sets in Y
            var addVelocityRedirectedDir = Vector3.RotateTowards(addVelocity.normalized,
                moveInputCameraRelative.normalized, Mathf.Deg2Rad * 30f, 0f);
            var addVelocityRedirected = addVelocity.magnitude * addVelocityRedirectedDir;
            
            return horizontalVelocity + verticalVelocity + addVelocityRedirected;
           
        }

        private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            
            float currentTurnSpeed;
            Vector2 horizontalVelocity = currentVelocity.xz();
            
            currentTurnSpeed = TurnSpeed.Value;
            
            /**************************************
             * Get New Move Direction
             **************************************/

            //Rotate current vel towards target vel to get new direction
            Vector2 dummyVel = Vector3.zero;
            Vector2 dir = Vector2.SmoothDamp(horizontalVelocity.normalized, moveInputCameraRelative.xz(),
                ref dummyVel, currentTurnSpeed);
            
            newDirection = dir.xoy().normalized;
            
            

            /**************************************
             * Get New Move Speed
             **************************************/

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
            
            //return newDirection * newSpeed;

            //return currentVelocity.xoz() + moveInputCameraRelative * 20f * Time.deltaTime;
            return currentVelocity.xoz();
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
            //Vector3 velocityDirection = new Vector3(cachedVelocity.Value.x, 0f, cachedVelocity.Value.z);
            //if (velocityDirection == Vector3.zero) velocityDirection = Vector3.forward;

            Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            Quaternion velocityRotation = Quaternion.LookRotation(moveInputCameraRelative.normalized,
                Vector3.up);

            if (Mathf.Approximately(moveInputCameraRelative.magnitude, 0f)) return;
            if (Mathf.Approximately(MoveInput.Value.magnitude, 0f)) return;

            currentRotation = Quaternion.RotateTowards(lookRotation, velocityRotation, RotationDeltaMax.Value/3f);

            NewRotationOut.Value = currentRotation;
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

            Color actualMoveColor = new Color(1f, 1f, 0f, .35f);
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
