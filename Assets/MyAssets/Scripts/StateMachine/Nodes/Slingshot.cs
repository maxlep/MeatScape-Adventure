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
        private FloatReference MinChargeTime;
        
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
        private FloatReference OptimalChargeMultiplier;
        
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
        private TriggerVariable SlingshotHomingReleaseInput;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        private TransformSceneReference SlingshotAimPivot;
        
        #endregion

        #region Outputs

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
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Events")] [Required]
        private GameEvent SlingShotReleaseEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Events")] [Required]
        private GameEvent SlingShotOptimalReleaseEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Events")] [Required]
        private TriggerVariable SlingShotOptimalReleaseTrigger;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Events")] [Required]
        private TriggerVariable SlingShotOptimalReleaseHomingTrigger;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Events")] [Required]
        private TriggerVariable SlingShotReleaseTrigger;
        
        #endregion


        private Vector3 accumulatedForce;
        private float enterTime = Mathf.NegativeInfinity;
        private bool activatedParticles;

        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();
            accumulatedForce = Vector3.zero;
            enterTime = Time.time;
            activatedParticles = false;
            SlingshotReleaseInput.Subscribe(Release);
            SlingshotHomingReleaseInput.Subscribe(ReleaseHoming);
        }

        public override void Exit()
        {
            base.Exit();
            SlingshotReleaseInput.Unsubscribe(Release);
            SlingshotHomingReleaseInput.Unsubscribe(ReleaseHoming);
            SlingshotAimPivot.Value.localRotation = Quaternion.identity;
        }

        public override void Execute()
        {
            base.Execute();

            //Only charge if past the min time
            if (enterTime + MinChargeTime.Value < Time.time)
            {
                playerController.ToggleArrow(true);
                AccumulateSlingForce();
            }
                

            if (!activatedParticles && enterTime + OptimalChargeTime.Value - OptimalChargeErrorThreshold.Value < Time.time)
            {
                SlingshotOptimalChargeEvent.Raise();
                activatedParticles = true;
            }

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
                slingDirection = FlattenDirectionOntoSlope(moveInputCameraRelative.normalized, groundingStatus.GroundNormal);
            
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
            accumulatedForce = slingDirection * accumulatedForceMagnitude;

            float maxArowLine = 15f;
            playerController.SetSlingshotArrow(percentToMax * maxArowLine * slingDirection);
            SlingshotDirection.Value = slingDirection;
            
        }

        private void Release()
        {
            Release(false);
        }

        private void ReleaseHoming()
        {
            Release(true);
        }

        private void Release(bool isHoming)
        {
            float timeToOptimalCharge = Mathf.Abs(Time.time - (enterTime + OptimalChargeTime.Value));
            if (timeToOptimalCharge < OptimalChargeErrorThreshold.Value)
            {
                accumulatedForce = MaxForce.Value * OptimalChargeMultiplier.Value * accumulatedForce.normalized;

                //Homing or normal release based on input
                if (isHoming)
                {
                    SlingShotOptimalReleaseHomingTrigger.Activate();
                }
                else
                {
                    SlingShotOptimalReleaseEvent.Raise();
                    SlingShotOptimalReleaseTrigger.Activate();
                }
                
            }
            else if (accumulatedForce != Vector3.zero)
            {
                SlingShotReleaseEvent.Raise();
                SlingShotReleaseTrigger.Activate();
            }
            
            playerController.AddImpulse(accumulatedForce);
            playerController.ToggleArrow(false);
            DelayTimer.StartTimer();
        }
    }
}