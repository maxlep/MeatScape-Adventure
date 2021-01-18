using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    
    public class Slingshot : BaseMovement
    {
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference MaxForce;
        
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [Required]
        private FloatReference TimeToMaxCharge;

        private Vector3 accumulatedForce;
        private float enterTime = Mathf.NegativeInfinity;
        
        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();
            enterTime = Time.time;
            playerController.ToggleArrow(true);
        }

        public override void Exit()
        {
            base.Exit();
            
            playerController.AddImpulse(accumulatedForce);
            playerController.ToggleArrow(false);
        }

        public override void Execute()
        {
            base.Execute();

            AccumulateSlingForce();

        }

        #endregion
        
        protected override Vector3 CalculateVelocity(VelocityInfo velocityInfo)
        {
            Vector3 currentVelocity = velocityInfo.currentVelocity;
            Vector3 impulseVelocity = velocityInfo.impulseVelocity;
            Vector3 impulseVelocityRedirectble = velocityInfo.impulseVelocityRedirectble;
            
            return Vector3.zero;
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