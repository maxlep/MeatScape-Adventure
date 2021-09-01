using System.Collections;
using System.Collections.Generic;
using Den.Tools;
using DotLiquid.Util;
using KinematicCharacterController;
using MyAssets.Graphs.StateMachine.Nodes;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
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
        private FloatReference TiltFacInfluence;

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
        protected FloatReference DragCoefficientVerticalUpwards;
        
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
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Inputs")] [Required]
        protected FloatReference GlideEnterImpulse;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Inputs")]
        private FloatReference TurnAngleLerpRate;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Inputs")]
        private FloatReference TiltAngleLerpRate;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Inputs")]
        private FloatReference TurnAngleMin;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Inputs")]
        private FloatReference TurnAngleMax;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Inputs")]
        private FloatReference TiltAngleMin;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Inputs")]
        private FloatReference TiltAngleMax;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Inputs")]
        private FloatReference MaxUpwardsTiltThresholdSpeed;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Inputs")] [Required] 
        private FloatReference HungerDecayTime;
        
        #endregion

        #region Outputs

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Outputs")] [Required]
        protected FloatReference HorizontalSpeedOut;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Outputs")] [Required]
        protected FloatReference SteeringFac;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Outputs")] [Required]
        protected FloatReference TiltFac;

        #endregion
        
        protected Vector3 previousVelocityOutput = Vector3.zero;
        private Vector3 tiltedDir;
        private float tiltAngle;
        private float turnAngle;
        private float lastHungerDecayTime;
        private bool firstVelocityUpdate;


        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();
            gravity *= GravityFactor.Value;
            playerController.AddImpulseOverlayed(playerController.transform.forward * GlideEnterImpulse.Value);
            playerController.IncrementHunger(-1);
            lastHungerDecayTime = Time.time;
            firstVelocityUpdate = true;
            turnAngle = 0f;
            tiltAngle = 0f;
        }

        public override void Exit()
        {
            base.Exit();
            GlidePivot.Value.localRotation = quaternion.identity;
        }

        public override void Execute()
        {
            base.Execute();
            
            //Hunger decay
            if (lastHungerDecayTime + HungerDecayTime.Value < Time.time)
            {
                playerController.IncrementHunger(-1);
                lastHungerDecayTime = Time.time;
            }
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

            //NOTE: Here the horizontal velocity is fed into the vertical velocity!!!
            Vector3 horizontalVelocity = CalculateHorizontalVelocity(currentVelocity, effectiveGroundNormal);
            Vector3 verticalVelocity = CalculateVerticalVelocity(horizontalVelocity, effectiveGroundNormal);  //This methods gravity works but horizontal tilted y is lost...
            //Vector3 verticalVelocity = CalculateVerticalVelocity(currentVelocity, effectiveGroundNormal);   //This method the horizontal tilted is correct but also overrides dir of previous frame's velocity...

            //Redirect impulseVelocityRedirectable if conditions met
            if (EnableRedirect && CheckRedirectConditions(impulseVelocityRedirectble))
                totalImpulse += CalculateRedirectedImpulse(impulseVelocityRedirectble);
            else
                totalImpulse += impulseVelocityRedirectble;

            //Let vertical velocity set the y because it includes the vertical velocity from the horizontalvelocity method
            resultingVelocity = horizontalVelocity.xoz() + verticalVelocity.oyo();
            resultingVelocity += totalImpulse;

            firstVelocityUpdate = false;
            previousVelocityOutput = resultingVelocity;
            return resultingVelocity;
        }

        protected Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity, Vector3 effectiveGroundNormal)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

            //TODO: This is terrible...please refactor
            float currentSpeed = (firstVelocityUpdate) ? currentVelocity.xoz().magnitude : currentVelocity.magnitude;
            var horizontalSpeed = currentVelocity.xoz().magnitude;

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
            SteeringFac.Value = steeringAngle / 90;   //Clamp at 90 for max friction

            #region Check Breaking

            bool isBreaking = false;
            
            //If projected move input is not in same direction as velocity, assuming breaking
            if (Vector3.Dot(dir.xoz(), moveDir) < AccelerateDotThreshold.Value)
            {
                SteeringFac.Value = 0f;
                isBreaking = true;
            }
            
            #endregion
            
            var turningFriction = (1 + (CoefficientOfTurningFriction.Value * SteeringFac.Value)) * Time.deltaTime;
            var dragHorizontal = horizontalSpeed * DragCoefficientHorizontal.Value * Time.deltaTime;
            newSpeed = currentSpeed - dragHorizontal - turningFriction;
            
            #endregion

            #region Update Pivot

            var turnAngleTarget = Vector3.SignedAngle(dir, moveDir, Vector3.up);
            turnAngleTarget = Mathf.Clamp(-turnAngleTarget, TurnAngleMin.Value, TurnAngleMax.Value);
            turnAngle = Mathf.Lerp(turnAngle, turnAngleTarget, TurnAngleLerpRate.Value * Time.deltaTime);
            
            //If breaking, dont change turn angle
            if (isBreaking) turnAngleTarget = GlidePivot.Value.localRotation.z;

            TiltFac.Value = (MoveInput.Value.y + 1f) / 2f;
            
            //To stop floating in air forever when tilted up and going slow
            //Need certain amount of speed to be able to tilt above horizontal
            
            
            if (horizontalSpeed < MaxUpwardsTiltThresholdSpeed.Value)
            {
                var maxTiltFac = horizontalSpeed / MaxUpwardsTiltThresholdSpeed.Value;
                var currentMaxTilt = Mathf.Lerp(2f, -1f, maxTiltFac);   //This is the most the player can currently tilt upwards (-1 being most tilt up)
                TiltFac.Value = Mathf.Max(currentMaxTilt, TiltFac.Value);   //Enforce the current max tilt
            }
                
            
            var tiltAngleTarget = Mathf.Lerp(TiltAngleMin.Value, TiltAngleMax.Value, TiltFac.Value);
            tiltAngle = Mathf.Lerp(tiltAngle, tiltAngleTarget, TiltAngleLerpRate.Value * Time.deltaTime);
            
            GlidePivot.Value.localRotation = Quaternion.Euler(tiltAngle,  GlidePivot.Value.localRotation.y, turnAngle);

            #endregion

            #region Tilt Direction

            // var tiltDirAmount = (MoveInput.Value.y + 1f) / 2f;
            // var tiltDirAngle = Mathf.Lerp(-60f, 60f, tiltDirAmount);
            // tiltDirAngle = Mathf.Lerp(tiltAngle, tiltAngleTarget, TiltAngleLerpRate.Value * Time.deltaTime);
            var playerRight = Vector3.Cross(currentVelocity.xoz().normalized, Vector3.up);
            newDirection = Quaternion.AngleAxis(-tiltAngle, playerRight) * newDirection;
            tiltedDir = newDirection;

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

            Vector3 newVelocity = currentVelocity.oyo();

            float gravityAirborn = gravity * GravityFactorAirborn.Value;

            if (newVelocity.y <= 0f)  //Falling
            {
                var drag = newVelocity.y * DragCoefficientVerticalDownwards.Value * Time.deltaTime;
                var dragFac = SteeringFac.Value * SteeringFacInfluence.Value + TiltFac.Value * TiltFacInfluence.Value;
                drag *= 1f / ((dragFac * DragDivisor.Value) + 1f);  //As drag fac increases, decrease drag to fall faster
                newVelocity.y -= drag;
                newVelocity.y += gravityAirborn * FallMultiplier.Value * Time.deltaTime;
            }
            else if (newVelocity.y > 0f)
            {
                var drag = newVelocity.y * DragCoefficientVerticalUpwards.Value * Time.deltaTime;
                newVelocity.y -= drag;
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
        
        public override void DrawGizmos()
        {
            if (playerController == null) return;
        
            // set up all static parameters. these are used for all following Draw.Line calls
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.LineThicknessSpace = ThicknessSpace.Meters;
            Draw.LineThickness = .1f;

            Vector3 startPos = playerController.transform.position;
            Vector3 endPos = playerController.transform.position + newDirection * (newSpeed / BaseSpeed.Value) * 10f;
            Vector3 endPos2 = playerController.transform.position + moveInputCameraRelative * 10f;
            Vector3 endPos4 = playerController.transform.position + tiltedDir * 10f;
            //Vector3 endPos6 = playerController.transform.position + playerController.GroundingStatus.GroundNormal * 10f;
            //Vector3 endPos7 = playerController.transform.position + intersectingVector * 10f;

            Color actualMoveColor = new Color(1f, 1f, 0f, .35f);
            Color moveInputColor = new Color(0f, 1f, 0f, .35f);
            Color projectedMoveInputColor = new Color(.3f, 1f, .8f, .35f);
            Color cachedVelocityColor = new Color(0f, 0f, 1f, .35f);
            Color slopeRightColor = Color.magenta;
            Color groundNormalColor = Color.white;

            Draw.Line(startPos, endPos, actualMoveColor); //Actual move
            Draw.Line(startPos, endPos2, moveInputColor); //Move Input
            Draw.Line(startPos, endPos4, cachedVelocityColor); //TiledDir
            //Draw.Line(startPos, endPos5, slopeRightColor); //Slope Right
            //Draw.Line(startPos, endPos6, groundNormalColor); //Ground Normal
            //Draw.Line(startPos, endPos7, slopeRightColor); //Intersecting Vector
            
            
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos, .25f, actualMoveColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos2, .25f, moveInputColor);
            Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos4, .25f, cachedVelocityColor);
            //Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos5, .25f, slopeRightColor);
            //Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos6, .25f, groundNormalColor);
            //Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, endPos7, .25f, slopeRightColor);
        }
}
