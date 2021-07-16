using KinematicCharacterController;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    
    public class Slingshot : RollMovement
    {
        #region Inputs

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private FloatReference MaxForce;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private FloatReference MinForce;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private FloatReference TimeToMaxCharge;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private FloatReference OptimalChargeTime;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private FloatReference OptimalChargeErrorThreshold;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private FloatReference HomingDotProductMin;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private TimerVariable DelayTimer;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private TransformSceneReference currentTargetSceneReference;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private TriggerVariable SlingshotReleaseInput;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private GameEvent SlingshotHomingReleaseInput;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private TransformSceneReference SlingshotAimPivot;
        
        #endregion

        #region Outputs

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Outputs")] [Required]
        private FloatReference TimeToOptimalCharge;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Outputs")] [Required]
        private Vector3Reference AccumulatedSlingshotForce;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Outputs")] [Required]
        private Vector3Reference SlingshotDirection;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Outputs")] [Required]
        private TransformSceneReference slingshotTargetSceneReference;

        #endregion
        
        #region Events
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Events")] [Required]
        private GameEvent SlingshotOptimalChargeEvent;

        #endregion


        private float enterTime = Mathf.NegativeInfinity;
        private bool activatedParticles;

        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();
            AccumulatedSlingshotForce.Value = Vector3.zero;
            slingshotTargetSceneReference.Value = null;
            enterTime = Time.time;
            activatedParticles = false;
        }

        public override void Exit()
        {
            base.Exit();
            SlingshotAimPivot.Value.localRotation = Quaternion.identity;
        }

        public override void Execute()
        {
            base.Execute();
            
            playerController.ToggleArrow(true);
            AccumulateSlingForce();

            if (!activatedParticles && enterTime + OptimalChargeTime.Value - OptimalChargeErrorThreshold.Value < Time.time)
            {
                SlingshotOptimalChargeEvent.Raise();
                activatedParticles = true;
            }
            
            TimeToOptimalCharge.Value = Mathf.Abs(Time.time - (enterTime + OptimalChargeTime.Value));
        }

        #endregion
        
        protected override Vector3 CalculateVelocity(VelocityInfo velocityInfo)
        {
            return base.CalculateVelocity(velocityInfo);
            
            //If not grounded yet, keep y velocity going until grounded
            if (!playerController.GroundingStatus.IsStableOnGround)
                return Vector3.down * 10f;

            return Vector3.zero;
        }

        protected override void UpdateRotation(Quaternion currentRotation)
        {
            var GroundingStatus = playerController.GroundingStatus;
            
            //If no move input, just face cam forward flattened onto slope
            if (Mathf.Approximately(0f, moveInputCameraRelative.sqrMagnitude))
            {
                Vector3 camDirOnSlope = FlattenDirectionOntoSlope(PlayerCameraTransform.Value.forward.xoz(), GroundingStatus.GroundNormal);
                
                //TODO: Figure out how to get pivot working properly, its not reseting on exit
                //Here the pivot should handle vertical tilt (visual) while actual rotaiton is just in XZ-Plane
                SlingshotAimPivot.Value.rotation = Quaternion.LookRotation(camDirOnSlope, GroundingStatus.GroundNormal);
                NewRotationOut.Value = Quaternion.LookRotation(camDirOnSlope.xoz(), Vector3.up);
                return;
            }

            Vector3 moveInputOnSlope;
                
            //If ground is flat, just take flattened move input
            if (GroundingStatus.FoundAnyGround && GroundingStatus.GroundNormal == Vector3.up)
                moveInputOnSlope = moveInputCameraRelative.xoz().normalized;
            else
                moveInputOnSlope = FlattenDirectionOntoSlope(moveInputCameraRelative.xoz().normalized, GroundingStatus.GroundNormal);

            //TODO: Figure out how to get pivot working properly, its not reseting on exit
            //Here the pivot should handle vertical tilt (visual) while actual rotaiton is just in XZ-Plane
            SlingshotAimPivot.Value.rotation = Quaternion.LookRotation(moveInputOnSlope, GroundingStatus.GroundNormal);
            NewRotationOut.Value = Quaternion.LookRotation(moveInputOnSlope.xoz(), Vector3.up);
        }

        private void AccumulateSlingForce()
        {
            CharacterGroundingReport groundingStatus = playerController.GroundingStatus;
            Vector3 slingDirection;
            
            //If no move input, just launch forward
            if (Mathf.Approximately(0f, moveInputCameraRelative.sqrMagnitude))
                slingDirection = FlattenDirectionOntoSlope(PlayerCameraTransform.Value.forward.xoz(), groundingStatus.GroundNormal);
            else
                slingDirection = FlattenDirectionOntoSlope(moveInputCameraRelative.xoz().normalized, groundingStatus.GroundNormal);
            
            slingshotTargetSceneReference.Value = null;
            
            if (currentTargetSceneReference.Value != null)
            {
                Vector3 playerToTarget =
                    (currentTargetSceneReference.Value.position - playerController.transform.position).normalized;

                if (Vector3.Dot(moveInputCameraRelative.xoz().normalized, playerToTarget.xoz().normalized) > HomingDotProductMin.Value)
                    slingshotTargetSceneReference.Value = currentTargetSceneReference.Value;
                
            }
            
            float timePassed = Time.time - enterTime;
            float percentToMax = Mathf.Clamp01(timePassed/TimeToMaxCharge.Value);
            float accumulatedForceMagnitude = Mathf.Lerp(MinForce.Value, MaxForce.Value, percentToMax);
            AccumulatedSlingshotForce.Value = slingDirection * accumulatedForceMagnitude;
            
            float maxArowLine = 15f;
            playerController.SetSlingshotArrow(percentToMax * maxArowLine * slingDirection);
            SlingshotDirection.Value = slingDirection;
            
        }
        
    }
}