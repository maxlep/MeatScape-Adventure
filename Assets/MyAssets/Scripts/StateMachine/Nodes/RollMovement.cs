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
        [TabGroup("Outputs")] [Required]
        protected FloatReference TurnFactor;
        
        #region Vertical Movement

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
        private FloatReference MaxFallSpeed;
        

        #endregion

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
            
            Vector3 totalImpulse = impulseVelocity + impulseVelocityRedirectble;
            Vector3 resultingVelocity;
            Vector3 horizontalVelocity = CalculateHorizontalVelocity(currentVelocity);
            Vector3 verticalVelocity = CalculateVerticalVelocity(currentVelocity);

            resultingVelocity = horizontalVelocity + verticalVelocity;
            resultingVelocity += totalImpulse;

            return resultingVelocity;
        }

        private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;

            var currentDir = currentVelocity.xoz().normalized;
            var steeringDir = moveInputCameraRelative;
            var steeringAngle = Vector3.Angle(currentDir, steeringDir);
            var steeringFac = steeringAngle / 180;
            
            
            TurnFactor.Value = Vector3.SignedAngle(currentDir, steeringDir, Vector3.up) / 30;

            // var aboveBaseSpeed = currentVelocity.magnitude > BaseSpeed.Value;

            var rollingFriction = PlayerWeight.Value * CoefficientOfRollingFriction.Value;
            var turningFriction = (1 + (CoefficientOfTurningFriction.Value * steeringFac));
            var friction = rollingFriction * turningFriction;
            
            Vector2 dummyVel = Vector3.zero;
            Vector2 dir = Vector2.SmoothDamp(currentDir.normalized, steeringDir,
                ref dummyVel, TurnSpeed.Value);

            newSpeed = Mathf.Max(currentVelocity.xoz().magnitude - friction, BaseSpeed.Value);

            newDirection = moveInputCameraRelative;

            return newDirection * newSpeed;
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
                    new Color(1f, 0.5f, 0, alpha))
            };

            foreach (var line in lines)
            {
                Draw.Line(startPos, line.endPos, line.color);
                Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, line.endPos, radius, line.color);
            }
        }
    }
}