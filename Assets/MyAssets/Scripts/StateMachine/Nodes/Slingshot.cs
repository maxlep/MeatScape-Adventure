using KinematicCharacterController;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    
    public class Slingshot : RollMovement
    {
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference MaxForce;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference MinForce;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference TimeToMaxCharge;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference MinChargeTime;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeTime;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeErrorThreshold;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeMultiplier;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private TimerVariable DelayTimer;
        
        #region GameEvents
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private GameEvent SlingshotOptimalChargeEvent;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private GameEvent SlingShotReleaseEvent;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private GameEvent SlingShotOptimalReleaseEvent;
        
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
        }

        public override void Exit()
        {
            base.Exit();

            float timeToOptimalCharge = Mathf.Abs(Time.time - (enterTime + OptimalChargeTime.Value));
            if (timeToOptimalCharge < OptimalChargeErrorThreshold.Value)
            {
                accumulatedForce = MaxForce.Value * OptimalChargeMultiplier.Value * accumulatedForce.normalized;
                SlingShotOptimalReleaseEvent.Raise();
            }
            else if (accumulatedForce != Vector3.zero)
            {
                SlingShotReleaseEvent.Raise();
            }
            
            playerController.AddImpulse(accumulatedForce);
            playerController.ToggleArrow(false);
            DelayTimer.StartTimer();
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
        }

        private void AccumulateSlingForce()
        {
            CharacterGroundingReport groundingStatus = playerController.GroundingStatus;
            
            Vector3 slingDirection =
                Vector3.ProjectOnPlane(moveInputCameraRelative.normalized, groundingStatus.GroundNormal);

            float timePassed = Time.time - enterTime;
            float percentToMax = Mathf.Clamp01(timePassed/TimeToMaxCharge.Value);
            float accumulatedForceMagnitude = Mathf.Lerp(MinForce.Value, MaxForce.Value, percentToMax);
            accumulatedForce = slingDirection * accumulatedForceMagnitude;

            float maxArowLine = 15f;
            playerController.SetSlingshotArrow(percentToMax * maxArowLine * slingDirection);
        }
    }
}