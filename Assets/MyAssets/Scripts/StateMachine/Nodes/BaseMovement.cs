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
        #region Horizontal Movement
        
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
        
        #endregion
        
        #region Vertical Movement

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")] 
        protected FloatReference TimeToJumpApex;

        [HideIf("$zoom")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical Movement")] 
        protected FloatReference MaxJumpHeight;
        
        #endregion
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
        protected Vector2Reference MoveInput;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
        protected Vector3Reference NewVelocityOut;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
        protected QuaternionReference NewRotationOut;

        protected Vector3 moveInputCameraRelative;
        protected Vector3 newDirection;
        protected float newSpeed;
        protected float gravity;
        protected KinematicCharacterMotor characterMotor;

        #region Lifecycle methods
        public override void Enter()
        {
            base.Enter();
            playerController.onStartUpdateVelocity += UpdateVelocity;
            playerController.onStartUpdateRotation += UpdateRotation;
            characterMotor = playerController.CharacterMotor;
            gravity = -(2 * MaxJumpHeight.Value) / Mathf.Pow(TimeToJumpApex.Value, 2);
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
        protected virtual void UpdateVelocity(VelocityInfo velocityInfo)
        {
            Vector3 currentVelocity = velocityInfo.currentVelocity;
            Vector3 impulseVelocity = velocityInfo.impulseVelocity;
            
            SetMoveDirection();

            var impulseVelocityDampened = impulseVelocity * (1 - ImpulseDampingFactor.Value);
            NewVelocityOut.Value = CalculateVelocity(velocityInfo);
        }
        
        protected virtual void UpdateRotation(Quaternion currentRotation)
        {
            if (Mathf.Approximately(moveInputCameraRelative.magnitude, 0f))
                return;

            NewRotationOut.Value = Quaternion.LookRotation(moveInputCameraRelative, Vector3.up);
        }
        #endregion
        
        private void SetMoveDirection()
        {
            Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
            moveInputCameraRelative = camForward.GetRelative(MoveInput.Value).xoy();
        }

        protected virtual Vector3 CalculateVelocity(VelocityInfo velocityInfo)
        {
            Vector3 currentVelocity = velocityInfo.currentVelocity;
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;

            var currentDir = currentVelocity.xz();
            
            return Vector3.zero;
        }

        /// <summary>
        /// Different than ProjectOnPlane. This method is like looking at the vector from top-down
        /// view and pushing it directly downwards onto the slope. Useful to get move input on slope that
        /// feels good.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="groundNormal"></param>
        /// <returns></returns>
        protected virtual Vector3 FlattenDirectionOntoSlope(Vector3 dir, Vector3 groundNormal)
        {
            Vector3 slopeRight = Vector3.Cross(Vector3.down, groundNormal).normalized;
            
            //Signed angle between slope right and camera right (to get camera and slope relative)
            float slopeRightToCamRight = Vector3.SignedAngle(slopeRight, 
                PlayerCameraTransform.Value.right.xoz(), Vector3.up);
            
            //Get angle of moveInput on Unit Circle (Degrees from right position)
            float moveInputAngle = dir.xz().normalized.AngleOnUnitCircle();

            //Combine input and slope/relative camera angle
            float camSlopeRelativeMoveAngle = slopeRightToCamRight - moveInputAngle;

            //Rotate the slope right around the slope normal by camSlopeRelativeMoveAngle
            //This essentially projects the XZ-bound camSlopeRelativeMoveAngle downwards onto the slope
            Vector3 projectedDirection = (Quaternion.AngleAxis(camSlopeRelativeMoveAngle, groundNormal)
                                  * slopeRight).normalized;

            return projectedDirection;
        }
    }
}