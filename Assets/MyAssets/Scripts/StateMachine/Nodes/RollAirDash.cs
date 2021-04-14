using Den.Tools;
using KinematicCharacterController;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class RollAirDash : PlayerStateNode
    {
        #region Inputs
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Inputs")] [Required]
        protected Vector2Reference MoveInput;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Horizontal")] [Required]
        protected TransformSceneReference PlayerCameraTransform;

        #endregion
        
        #region Horizontal Movement

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference AirForwardForce;

        #endregion

        #region Vertical Movement

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference AirUpwardForce;
        
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
            
            Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
            moveInputCameraRelative = camForward.GetRelative(MoveInput.Value).xoy();
            
            AirRollDashEvent.Raise();
            StoredJumpVelocity.Value = AirUpwardForce.Value;
            Vector3 jumpDirection;
                
            //If no move input, just launch forward
            if (Mathf.Approximately(0f, moveInputCameraRelative.sqrMagnitude))
                jumpDirection = PlayerCameraTransform.Value.forward.xoz();
            else
                jumpDirection = moveInputCameraRelative.xoz().normalized;
                
                
            playerController.AddImpulseOverlayed(jumpDirection * AirForwardForce.Value, true);
            
        }

        #endregion
       
    }
}