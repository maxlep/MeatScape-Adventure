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
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference MaxForce;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference MinForce;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference TimeToMaxCharge;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference MinChargeTime;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeTime;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeErrorThreshold;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeMultiplier;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private TimerVariable DelayTimer;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private Vector3Reference SlingshotDirection;
        
        #region GameEvents
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private GameEvent SlingshotOptimalChargeEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private GameEvent SlingShotReleaseEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private GameEvent SlingShotOptimalReleaseEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private TriggerVariable SlingShotOptimalReleaseTrigger;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private TriggerVariable SlingShotReleaseTrigger;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private TriggerVariable SlingshotReleaseInput;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private TransformSceneReference currentTargetSceneReference;
        
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
        }

        public override void Exit()
        {
            base.Exit();
            SlingshotReleaseInput.Unsubscribe(Release);
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
            //return base.CalculateVelocity(velocityInfo);

            return Vector3.zero;
        }

        protected override void UpdateRotation(Quaternion currentRotation)
        {
            var GroundingStatus = playerController.GroundingStatus;

            Vector3 moveInputOnSlope;
                
            //If ground is flat, just take flattened move input
            if (GroundingStatus.FoundAnyGround && GroundingStatus.GroundNormal == Vector3.up)
                moveInputOnSlope = moveInputCameraRelative.xoz().normalized;
            else
                moveInputOnSlope = FlattenMoveInputOntoSlope(MoveInput.Value, GroundingStatus.GroundNormal);
            
            NewRotationOut.Value = Quaternion.LookRotation(moveInputOnSlope, GroundingStatus.GroundNormal);
        }

        private void AccumulateSlingForce()
        {
            CharacterGroundingReport groundingStatus = playerController.GroundingStatus;

            Vector3 slingDirection;
            
            if (currentTargetSceneReference.Value == null)
                slingDirection = Vector3.ProjectOnPlane(moveInputCameraRelative.normalized, groundingStatus.GroundNormal);
            else
            {
                Vector3 playerToTarget =
                    (currentTargetSceneReference.Value.position - playerController.transform.position).normalized;
                slingDirection = playerToTarget;
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
            float timeToOptimalCharge = Mathf.Abs(Time.time - (enterTime + OptimalChargeTime.Value));
            if (timeToOptimalCharge < OptimalChargeErrorThreshold.Value)
            {
                accumulatedForce = MaxForce.Value * OptimalChargeMultiplier.Value * accumulatedForce.normalized;
                SlingShotOptimalReleaseEvent.Raise();
                SlingShotOptimalReleaseTrigger.Activate();
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