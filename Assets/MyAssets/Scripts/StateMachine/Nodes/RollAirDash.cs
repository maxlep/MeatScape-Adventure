using Den.Tools;
using KinematicCharacterController;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class RollAirDash : PlayerStateNode
    {
        private bool isAdditive = false;
        
        #region Inputs
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
        protected Vector2Reference MoveInput;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
        protected Vector3Reference PreviousVelocity;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal")] [Required]
        protected TransformSceneReference PlayerCameraTransform;

        #endregion

        #region Horizontal Movement

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatValueReference AirForwardForce;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatValueReference AirForwardForceAdditive;

        #endregion

        #region Vertical Movement

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatValueReference AirUpwardForce;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatValueReference AirUpwardForceAdditive;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference StoredJumpVelocity;
        
        #endregion

        #region GameEvents

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Events")] [Required]
        protected GameEvent AirRollDashEvent;

        #endregion
        
        #region Lifecycle methods
        
        protected Vector3 moveInputCameraRelative;


        public override void Enter()
        {
            base.Enter();
            
            AirRollDashEvent.Raise();

            Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
            moveInputCameraRelative = camForward.GetRelative(MoveInput.Value).xoy();

            #region Vertical

            if (isAdditive)
                playerController.AddImpulseOverlayed(Vector3.up * AirUpwardForceAdditive.Value, false);
            else
                StoredJumpVelocity.Value = AirUpwardForce.Value;

            #endregion


            #region Horizontal

            Vector3 jumpDirectionHorizontal;

            jumpDirectionHorizontal = moveInputCameraRelative.xoz().normalized * MoveInput.Value.magnitude;

            if (isAdditive)
            {
                //Add current velocity to cancel it out
                Vector3 prevVelocity = PreviousVelocity.Value;
                playerController.AddImpulseOverlayed(jumpDirectionHorizontal * (AirForwardForceAdditive.Value + prevVelocity.magnitude), true);
            }
                
            else
                playerController.AddImpulseOverlayed(jumpDirectionHorizontal * AirForwardForce.Value, true);

            #endregion
        }

        #endregion
        

        #region Transition Methods

        public void SetAdditive(bool additive)
        {
            isAdditive = additive;
        }

        #endregion
       
    }
}