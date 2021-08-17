using System.Collections;
using System.Collections.Generic;
using DotLiquid.Util;
using KinematicCharacterController;
using MyAssets.Graphs.StateMachine.Nodes;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

public class Glide : BaseMovement
{
    #region Horizontal Movement

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference SlowTurnThreshold;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference SlowTurnSpeed;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference AccelerateDotThreshold;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference CoefficientOfTurningFriction;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference DragCoefficientHorizontal;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference GlideIdleAcceleratePercent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference TimeToTargetSpeed;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference BreakingDrag;

        #endregion

        #region Vertical Movement

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        private FloatReference FallMultiplier;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        private FloatReference DragDivisor;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        private FloatReference SteeringFacInfluence;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        private FloatReference BaseSpeedFacInfluence;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        private FloatReference UpwardsGravityMultiplier;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        private FloatReference MaxFallSpeed;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference DragCoefficientVerticalDownwards;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference GravityFactor;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference GravityFactorAirborn;
        

        #endregion
        
        #region Inputs

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Inputs")] [Required]
        protected TransformSceneReference GlidePivot;

        #endregion

        #region Outputs

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Outputs")] [Required]
        protected FloatReference HorizontalSpeedOut;

        #endregion
        
        protected Vector3 previousVelocityOutput = Vector3.zero;
        private float steeringFac;
        private float baseSpeedFac;

        
        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();
            gravity *= GravityFactor.Value;
        }

        public override void Exit()
        {
            base.Exit();
            GlidePivot.Value.localRotation = quaternion.identity;
        }

        public override void Execute()
        {
            base.Execute();
        }

        #endregion
        
        
        protected override Vector3 CalculateVelocity(VelocityInfo velocityInfo)
        {
            Vector3 currentVelocity = velocityInfo.currentVelocity;
            Vector3 impulseVelocity = velocityInfo.impulseVelocity;
            Vector3 impulseVelocityRedirectble = velocityInfo.impulseVelocityRedirectble;

            Vector3 totalImpulse = impulseVelocity;
            Vector3 resultingVelocity;

            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

            float currentVelocityMagnitude = currentVelocity.magnitude;
            KinematicCharacterMotor motor = playerController.CharacterMotor;

            #region Effective Normal & Reorient Vel on Slope

            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;

            if (motor.GroundingStatus.FoundAnyGround)
            {
                //Get effective ground normal based on move direction
                effectiveGroundNormal = CalculateEffectiveGroundNormal(currentVelocity, currentVelocityMagnitude, motor);

                // Reorient velocity on slope
                currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
            }

            #endregion

            Vector3 horizontalVelocity = CalculateHorizontalVelocity(currentVelocity, effectiveGroundNormal);
            Vector3 verticalVelocity = CalculateVerticalVelocity(currentVelocity, effectiveGroundNormal);

            //Redirect impulseVelocityRedirectble if conditions met
            if (EnableRedirect && CheckRedirectConditions(impulseVelocityRedirectble))
                totalImpulse += CalculateRedirectedImpulse(impulseVelocityRedirectble);
            else
                totalImpulse += impulseVelocityRedirectble;

            resultingVelocity = horizontalVelocity + verticalVelocity;
            resultingVelocity += totalImpulse;
            
            
            previousVelocityOutput = resultingVelocity;
            return resultingVelocity;
        }

        protected Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity, Vector3 effectiveGroundNormal)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

            float currentSpeed = currentVelocity.xoz().magnitude;

            #region Get New Move Direction

            //Rotate current vel towards target vel to get new direction
            Vector3 dummyVel = Vector3.zero;
            Vector3 dir;

            var slowTurnThreshold = SlowTurnThreshold.Value;
            var percentToSlowTurnSpeed = Mathf.InverseLerp(0f, slowTurnThreshold, currentSpeed);
            
            float currentTurnSpeed;

            //Lerp from turn speed to slow turn speed based on current velocity

            currentTurnSpeed = Mathf.Lerp(TurnSpeed.Value, SlowTurnSpeed.Value, percentToSlowTurnSpeed);

            Vector3 currentDir = currentVelocity.xoz().normalized;

            dir = Vector3.SmoothDamp(currentDir, moveInputCameraRelative.xoz().normalized,
                ref dummyVel, currentTurnSpeed).normalized;

            newDirection = dir;

            #endregion
            
            #region Get New Speed

            var moveDir = moveInputCameraRelative.xoz().normalized;
            var steeringAngle = Mathf.Clamp(Vector3.Angle(dir, moveDir), 0f, 90f);
            steeringFac = steeringAngle / 90;   //Clamp at 90 for max friction

            //TurnFactor.Value = Vector3.SignedAngle(horizontalDir, steeringDir, Vector3.up) / 30;
            
            var turningFriction = (1 + (CoefficientOfTurningFriction.Value * steeringFac));

            var drag = currentSpeed * DragCoefficientHorizontal.Value * Time.deltaTime;

            #region Accelerate to target speed

            float targetMagnitude;
            float currentMagnitude = Mathf.Clamp01(currentSpeed / BaseSpeed.Value);
            bool isBreaking = false;
            
            //If move input is zero, take current speed and start accelerating forward
            if (Mathf.Approximately(0f, MoveInput.Value.magnitude))
            {
                targetMagnitude = currentMagnitude + GlideIdleAcceleratePercent.Value * Time.deltaTime;
            }

            //If projected move input is in same direction as velocity, set target speed
            else if (Vector3.Dot(dir.xoz(), moveDir) > AccelerateDotThreshold.Value)
                targetMagnitude = 1f;

            //Otherwise, assume player wants to stop and turn around 
            else
            {
                targetMagnitude = 0f;
                steeringFac = 0f;
                isBreaking = true;
            }

            
            targetMagnitude = Mathf.Clamp01(targetMagnitude);
            
            var targetSpeed = BaseSpeed.Value * targetMagnitude;
            if (newSpeed < targetSpeed)
            {
                //TimeToTargetSpeed = Time to get to target speed from 0
                newSpeed = currentSpeed + targetSpeed * Time.deltaTime / TimeToTargetSpeed.Value;
            }
            else
            {
                //Only apply drag if not accelerating to base speed
                newSpeed = currentSpeed - drag - turningFriction;
            }

            if (isBreaking)
                newSpeed -= BreakingDrag.Value * newSpeed * Time.deltaTime;

            if (BaseSpeed.Value != 0f)
                baseSpeedFac = Mathf.Clamp01(newSpeed / BaseSpeed.Value);
            
            
            #endregion
            
            #endregion

            #region Update Pivot

            var turnAngle = Vector3.SignedAngle(dir, moveDir, Vector3.up);
            turnAngle = Mathf.Clamp(-turnAngle, -40f, 40f);
            
            //If breaking, dont change turn angle
            if (isBreaking) turnAngle = GlidePivot.Value.localRotation.z;
            
            var tiltAngle = Mathf.Lerp(-45f, 20f, baseSpeedFac);
            GlidePivot.Value.localRotation = Quaternion.Euler(tiltAngle,  GlidePivot.Value.localRotation.y, turnAngle);

            #endregion
            
            HorizontalSpeedOut.Value = newSpeed;
            return newDirection * newSpeed;
        }

        protected Vector3 CalculateVerticalVelocity(Vector3 currentVelocity, Vector3 effectiveGroundNormal)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

            //Return if standing on flat ground
            if (GroundingStatus.FoundAnyGround && effectiveGroundNormal == Vector3.up)
                return Vector3.zero;

            #region Airborn

            Vector3 newVelocity = currentVelocity.y * Vector3.up;

            float gravityAirborn = gravity * GravityFactorAirborn.Value;

            if (newVelocity.y <= 0f)  //Falling
            {
                var drag = newVelocity.y * DragCoefficientVerticalDownwards.Value * Time.deltaTime;
                var dragFac = steeringFac * SteeringFacInfluence.Value + baseSpeedFac * BaseSpeedFacInfluence.Value;
                drag *= 1f / ((dragFac * DragDivisor.Value) + 1f);
                newVelocity.y -= drag;
                newVelocity.y += gravityAirborn * FallMultiplier.Value * Time.deltaTime;
            }
            else if (newVelocity.y > 0f)
            {
                newVelocity.y += gravityAirborn * UpwardsGravityMultiplier.Value * Time.deltaTime;
            }

            if (newVelocity.y < -Mathf.Abs(MaxFallSpeed.Value))   //Cap Speed
            {
                newVelocity.y = -Mathf.Abs(MaxFallSpeed.Value);
            }
            
            
            #endregion

            return newVelocity;
        }

        protected override void UpdateRotation(Quaternion currentRotation)
        {
            Quaternion newRotation;
            Vector3 lookDirection = playerController.transform.forward;
            Vector3 velocityDirection = playerController.CharacterMotor.Velocity.normalized;
            
            GroundingInfo groundInfo = playerController.GroundInfo;
            
            if (Mathf.Approximately(velocityDirection.magnitude, 0f))
                newRotation = currentRotation;
            else
                newRotation = Quaternion.LookRotation(velocityDirection.xoz(), Vector3.up);
                //newRotation = Quaternion.LookRotation(velocityDirection, playerController.GroundingStatus.GroundNormal);
            
            
            NewRotationOut.Value = newRotation;
        }
        
        protected override void UpdateGravityParameters()
        {
            playerController.Gravity = gravity * GravityFactorAirborn.Value;
            playerController.UpwardsGravityFactor = UpwardsGravityMultiplier.Value ;
            playerController.DownwardsGravityFactor = FallMultiplier.Value;
        }
}
