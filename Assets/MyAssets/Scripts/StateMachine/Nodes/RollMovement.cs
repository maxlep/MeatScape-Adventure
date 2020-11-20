using KinematicCharacterController;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class RollMovement : BaseMovement
    {
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal Movement")] [Required]
        protected FloatReference CoefficientOfRollingFriction;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal Movement")] [Required]
        protected FloatReference CoefficientOfTurningFriction;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal Movement")] [Required]
        protected FloatReference DragCoefficientHorizontal;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Outputs")] [Required]
        protected FloatReference TurnFactor;

        #region Vertical Movement

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")]
        private FloatReference FallMultiplier;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")]
        private FloatReference MaxFallSpeed;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical Movement")] [Required]
        protected FloatReference DragCoefficientVertical;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical Movement")] [Required]
        protected FloatReference HorizontalGravityFactor;

        
        
        #endregion

        private Vector3 velocityAlongSlope;
        private Vector3 moveInputOnSlope;

        public override void Enter()
        {
            base.Enter();
            
            playerController.GiveThrowKnockback(NewVelocityOut.Value.normalized);
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
            
            resultingVelocity = horizontalVelocity + verticalVelocity;
            resultingVelocity += totalImpulse;

            return resultingVelocity;
        }

        private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            
            #region Get New Move Direction

            //Rotate current vel towards target vel to get new direction
            Vector3 dirOnSlope = Vector3.ProjectOnPlane(currentVelocity.normalized, 
                GroundingStatus.GroundNormal);
            moveInputOnSlope = Vector3.ProjectOnPlane(moveInputCameraRelative.normalized, 
                GroundingStatus.GroundNormal);
            
            Vector3 dummyVel = Vector3.zero;
            
            Vector3 dir;

            if (GroundingStatus.FoundAnyGround)
            {
                dir = Vector3.SmoothDamp(dirOnSlope, moveInputOnSlope,
                    ref dummyVel, TurnSpeed.Value).normalized;
            }
            else
            {
                dir = Vector3.SmoothDamp(currentVelocity.xoz().normalized, moveInputCameraRelative.normalized,
                    ref dummyVel, TurnSpeed.Value).normalized;
            }
            
            
            
            newDirection = dir;

            #endregion
            
            #region Get New Speed

            Vector3 horizontalDir = dir.xoz().normalized;
            var steeringDir = moveInputCameraRelative;
            var steeringAngle = Vector3.Angle(horizontalDir, steeringDir);
            var steeringFac = steeringAngle / 180;
            
            
            TurnFactor.Value = Vector3.SignedAngle(horizontalDir, steeringDir, Vector3.up) / 30;

            // var aboveBaseSpeed = currentVelocity.magnitude > BaseSpeed.Value;

            //TODO: Dont apply friction in air?
            var rollingFriction = PlayerMass.Value * CoefficientOfRollingFriction.Value;
            var turningFriction = (1 + (CoefficientOfTurningFriction.Value * steeringFac));
            var friction = (GroundingStatus.FoundAnyGround) ?
                rollingFriction * turningFriction * Time.deltaTime : 0f;
            
            
            velocityAlongSlope = Vector3.ProjectOnPlane(currentVelocity, GroundingStatus.GroundNormal);
            var currentSpeed = (GroundingStatus.FoundAnyGround) ?
                velocityAlongSlope.magnitude : currentVelocity.xoz().magnitude;

            var drag = currentSpeed * DragCoefficientHorizontal.Value * Time.deltaTime;

            newSpeed = Mathf.Max(currentSpeed - friction - drag, BaseSpeed.Value);
            
            #endregion

            return newDirection * newSpeed;
        }

        private Vector3 CalculateVerticalVelocity(Vector3 currentVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;
            
            //Return if standing on flat ground
            if (GroundingStatus.FoundAnyGround && GroundingStatus.GroundNormal == Vector3.up)
                return Vector3.zero;

            #region Airborn

            Vector3 newVelocity = currentVelocity.y * Vector3.up;
            
            if (newVelocity.y <= 0 || GroundingStatus.FoundAnyGround)  //Falling
            {
                newVelocity.y += gravity * (FallMultiplier.Value - 1) * Time.deltaTime;
            }
            else if (newVelocity.y > 0f) //Drag when moving up (Note: Affects going up slopes)
            {
                var drag = -(newVelocity.y * newVelocity.y) * DragCoefficientVertical.Value * Time.deltaTime;
                newVelocity.y += drag + gravity * Time.deltaTime;
            }
            else
            {
                newVelocity.y += gravity * Time.deltaTime;
            }
        
            if (newVelocity.y < -Mathf.Abs(MaxFallSpeed.Value))   //Cap Speed
            {
                newVelocity.y = -Mathf.Abs(MaxFallSpeed.Value);
            }

            #endregion

            #region Grounded (slope)


            //If grounded, OVERRIDE and project onto ground plane (sloped to some degree)
            //Less gravity when applied horizontally
            if (GroundingStatus.FoundAnyGround)
            {
                newVelocity.y = gravity * Time.deltaTime;
                newVelocity = Vector3.ProjectOnPlane(newVelocity, GroundingStatus.GroundNormal);
                newVelocity *= HorizontalGravityFactor.Value;
            }

            #endregion

            return newVelocity;
        }
        
        protected override void UpdateRotation(Quaternion currentRotation)
        {
            Quaternion newRotation;
            Vector3 lookDirection = playerController.transform.forward;
            Vector3 velocityDirection = NewVelocityOut.Value.normalized;

            if (Mathf.Approximately(velocityDirection.magnitude, 0f))
            {
                newRotation = Quaternion.LookRotation(lookDirection, Vector3.up);;
            }
            else
            {
                newRotation = Quaternion.LookRotation(velocityDirection, Vector3.up);
            }

            NewRotationOut.Value = newRotation;
        }

        public override void DrawGizmos()
        {
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
                    new Color(1f, 0.5f, 0, alpha)),
                (startPos + moveInputOnSlope,
                    new Color(.6f, 0.5f, .9f, alpha))
            };

            foreach (var line in lines)
            {
                Draw.Line(startPos, line.endPos, line.color);
                Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, line.endPos, radius, line.color);
            }
        }
    }
}