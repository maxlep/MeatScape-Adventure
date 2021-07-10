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
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal")] [Required]
        protected FloatReference BaseSpeed;
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal")] [Required]
        protected FloatReference TurnSpeed;
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal")] [Required]
        protected FloatReference ImpulseDampingFactor;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal")] [Required]
        protected TransformSceneReference PlayerCameraTransform;
        
        #endregion
        
        #region Vertical Movement

        [HideIf("$collapsed")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical")] 
        protected FloatReference TimeToJumpApex;

        [HideIf("$collapsed")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical")] 
        protected FloatReference MaxJumpHeight;
        
        #endregion
        
        #region AddImpulse Redirect

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Redirect")] [Required]
        protected bool EnableRedirect;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Redirect")] [ShowIf("$EnableRedirect")]
        [Required]
        protected FloatReference RedirectMaxDegrees;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Redirect")] [ShowIf("$EnableRedirect")]
        [Required]
        protected FloatReference RedirectAngleThreshold;

        #endregion
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
        protected Vector2Reference MoveInput;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
        protected Vector3Reference NewVelocityOut;
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
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

        protected bool CheckRedirectConditions(Vector3 impulseVelocity)
        {
            if (Mathf.Approximately(impulseVelocity.magnitude, 0f))
                return false;
            
            float addImpulseToMoveInputDegrees =
                Vector3.Angle(impulseVelocity.normalized, moveInputCameraRelative.normalized);

            //If angle less than threshhold, redirect is valid
            if (addImpulseToMoveInputDegrees <= RedirectAngleThreshold.Value && 
                !Mathf.Approximately(impulseVelocity.magnitude, 0f) &&
                !Mathf.Approximately(moveInputCameraRelative.magnitude, 0f))
            {
                return true;
            }

            return false;
        }

        protected Vector3 CalculateEffectiveGroundNormal(Vector3 currentVelocity, float currentVelocityMagnitude, KinematicCharacterMotor motor)
        {
            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;
            if (currentVelocityMagnitude > 0f && motor.GroundingStatus.SnappingPrevented)
            {
                // Take the normal from where we're coming from
                Vector3 groundPointToCharacter = motor.TransientPosition - motor.GroundingStatus.GroundPoint;
                if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
                {
                    effectiveGroundNormal = motor.GroundingStatus.OuterGroundNormal;
                }
                else
                {
                    effectiveGroundNormal = motor.GroundingStatus.InnerGroundNormal;
                }
            }

            return effectiveGroundNormal;
        }

        protected Vector3 CalculateRedirectedImpulse(Vector3 addImpulse)
        {
            //Make it range [0, maxDegrees] based on move input
            float reditectDegrees = RedirectMaxDegrees.Value * moveInputCameraRelative.magnitude; 
            Vector3 addImpulseRedirectedDir = Vector3.RotateTowards(addImpulse.normalized,
                moveInputCameraRelative.normalized, reditectDegrees * Mathf.Deg2Rad, 0f);
            Vector3 addImpulseRedirected = addImpulse.magnitude * addImpulseRedirectedDir;
            
            //Preserve the original y velocity (so still reach same height)
            addImpulseRedirected.y = addImpulse.y; 
            
            return addImpulseRedirected;
        }

        /// <summary>
        /// Different than ProjectOnPlane. This method is like looking at the vector from top-down
        /// view and pushing it directly downwards onto the slope. Useful to get move input on slope that
        /// feels better than Vector3.ProjectOnPlane
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="slopeNormal"></param>
        /// <returns></returns>
        protected virtual Vector3 FlattenDirectionOntoSlope(Vector3 dir, Vector3 slopeNormal)
        {
            //The idea here is to create a plane from dir and Vector3.up
            //The dir flattened onto the slope (desired value) is nothing more than the line of intersection
            //between the slope plane and this newly created plane
            //To find the line of intersection of 2 planes, you just take the cross product of their normals
            Vector3 crossPlaneNormal = Vector3.Cross(Vector3.up, dir);
            Vector3 flattenedDirection = Vector3.Cross(crossPlaneNormal, slopeNormal).normalized;
            
            return flattenedDirection;
        }
        
    }
}