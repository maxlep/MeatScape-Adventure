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
    }
}