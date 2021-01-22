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
        private FloatReference TimeToMaxCharge;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeTime;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeErrorThreshold;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference OptimalChargeMultiplier;
        
        #region GameEvents
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private GameEvent SlingshotOptimalChargeEvent;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private GameEvent SlingshotOptimalChargeExecuteEvent;
        
        #endregion


        private Vector3 accumulatedForce;
        private float enterTime = Mathf.NegativeInfinity;
        private bool activatedParticles;
        
        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();
            enterTime = Time.time;
            activatedParticles = false;
            playerController.ToggleArrow(true);
        }

        public override void Exit()
        {
            base.Exit();

            float timeToOptimalCharge = Mathf.Abs(Time.time - (enterTime + OptimalChargeTime.Value));
            if (timeToOptimalCharge < OptimalChargeErrorThreshold.Value)
            {
                accumulatedForce = MaxForce.Value * OptimalChargeMultiplier.Value * accumulatedForce.normalized;
                SlingshotOptimalChargeExecuteEvent.Raise();
            }
            
            playerController.AddImpulse(accumulatedForce);
            playerController.ToggleArrow(false);
        }

        public override void Execute()
        {
            base.Execute();

            AccumulateSlingForce();

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
            Vector3 slingDirection = moveInputCameraRelative.normalized;

            float timePassed = Time.time - enterTime;
            float percentToMax = Mathf.Clamp01(timePassed/TimeToMaxCharge.Value);
            float accumulatedForceMagnitude = Mathf.Lerp(0f, MaxForce.Value, percentToMax);
            accumulatedForce = slingDirection * accumulatedForceMagnitude;

            float maxArowLine = 15f;
            playerController.SetSlingshotArrow(percentToMax * maxArowLine * slingDirection);
        }
    }
}