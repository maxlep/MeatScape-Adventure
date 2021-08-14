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
    public class RollMovement : BaseMovement
    {
        #region Horizontal Movement
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference TurnSpeedAir;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference SlowTurnThreshold;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference SlowTurnFactor;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference CoefficientOfRollingFriction;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference CoefficientOfTurningFriction;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference DragCoefficientHorizontal;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected bool EnableDeflect = true;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference DeflectFactor;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference DeflectThresholdVelocity;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected FloatReference DeflectContactDotThreshold;

        #endregion

        #region Vertical Movement
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        protected bool AlignColliderWithGroundNormal = true;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        protected FloatReference FallMultiplier;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        protected FloatReference UpwardsGravityMultiplier;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [Required] [TabGroup("Vertical")]
        protected FloatReference MaxFallSpeed;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference DragCoefficientVertical;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference GravityFactor;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference GravityFactorAirborn;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected bool EnableBounce = true;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference BounceFactor;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference BounceThresholdVelocity;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference BounceGroundDotThreshold;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference SlopeVerticalGravityFactor;

        #endregion
        
        #region Grounding

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Grounding")] [Required]
        protected FloatReference GroundStickAngleInputDownwards;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Grounding")] [Required]
        protected FloatReference GroundStickAngleInputUpwards;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Grounding")] [Required]
        protected FloatReference GroundStickAngleOutput;

        #endregion

        #region Inputs

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        protected BoolReference RollInputPressed;

        #endregion

        #region Outputs

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Outputs")] [Required]
        protected FloatReference HorizontalSpeedOut;

        #endregion

        #region GameEvents

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Events")] [Required]
        protected GameEvent PlayParticlesGameEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Events")] [Required]
        protected GameEvent StopParticlesGameEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Events")] [Required]
        protected GameEvent BounceGameEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Events")] [Required]
        protected GameEvent DeflectGameEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Events")] [Required]
        protected GameEvent RollSoundGameEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Inputs")] [Required]
        protected DynamicGameEvent PlayerCollidedWith;

        #endregion

        private Vector3 velocityAlongSlope;
        private Vector3 moveInputOnSlope;
        protected Vector3 previousVelocityOutput = Vector3.zero;
        private Vector3 storedDeflectVelocity;

        private float lastRollSoundTime;


        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();

            PlayerCollidedWith.Subscribe(CheckForDeflect);
            lastRollSoundTime = Mathf.NegativeInfinity;
            InitRollParticles();
            gravity *= GravityFactor.Value;
        }

        public override void Exit()
        {
            base.Exit();
            StopParticlesGameEvent.Raise();
            PlayerCollidedWith.Unsubscribe(CheckForDeflect);
        }

        public override void Execute()
        {
            base.Execute();
            HandleUpdateParticles();
            HandleRollSound();
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


            #region Bounce

            //Bounce if just became grounded
            Vector3 velocityIntoGround = Vector3.Project(previousVelocityOutput, -GroundingStatus.GroundNormal);
            
            float velocityGroundDot = Vector3.Dot(previousVelocityOutput.normalized, GroundingStatus.GroundNormal);

            //If roll pressed, dont bounce, instead will want to slingshot
            if (EnableBounce &&
                !RollInputPressed.Value &&
                !LastGroundingStatus.FoundAnyGround &&
                GroundingStatus.FoundAnyGround &&
                velocityIntoGround.magnitude >= BounceThresholdVelocity.Value &&
                -velocityGroundDot > BounceGroundDotThreshold.Value)
            {
                playerController.UngroundMotor();
                BounceGameEvent.Raise();

                //Reflect velocity perfectly then dampen the y based on dot with normal
                Vector3 reflectedVelocity = Vector3.Reflect(previousVelocityOutput, GroundingStatus.GroundNormal);
                reflectedVelocity.y *= BounceFactor.Value;
                
                //Redirect bounce if conditions met
                if (EnableRedirect && CheckRedirectConditions(reflectedVelocity))
                    reflectedVelocity = CalculateRedirectedImpulse(reflectedVelocity);
                

                resultingVelocity = reflectedVelocity;
            }

            #endregion
            
            #region Ground Stick Angle

            if (resultingVelocity.y >= 0)
                GroundStickAngleOutput.Value = GroundStickAngleInputUpwards.Value;
            else
                GroundStickAngleOutput.Value = GroundStickAngleInputDownwards.Value;

            #endregion
            
            previousVelocityOutput = resultingVelocity;
            return resultingVelocity;
        }

        private Vector3 dirOnSlopeGizmo;

        protected Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity, Vector3 effectiveGroundNormal)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

            if (!Mathf.Approximately(0f, storedDeflectVelocity.sqrMagnitude))
            {
                currentVelocity = storedDeflectVelocity;
            }
            
            velocityAlongSlope = Vector3.ProjectOnPlane(currentVelocity, effectiveGroundNormal);
            float currentSpeed = (GroundingStatus.FoundAnyGround)
                ? velocityAlongSlope.magnitude
                : currentVelocity.xoz().magnitude;
            
            //If just became grounded, redirect velocity and overwrite speed (dont slow down)
            if (!LastGroundingStatus.FoundAnyGround && GroundingStatus.FoundAnyGround)
                currentSpeed = previousVelocityOutput.magnitude;

            #region Get New Move Direction

            //Rotate current vel towards target vel to get new direction
            Vector3 dummyVel = Vector3.zero;
            Vector3 dir;

            var slowTurnSpeed = TurnSpeed.Value * SlowTurnFactor.Value;
            var slowTurnThreshold = SlowTurnThreshold.Value;
            var percentToSlowTurnSpeed = Mathf.InverseLerp(0f, slowTurnThreshold, currentSpeed);
            
            float currentTurnSpeed;

            //If grounded, lerp from turn speed to slow turn speed based on current velocity
            if (GroundingStatus.FoundAnyGround)
            {
                currentTurnSpeed = Mathf.Lerp(TurnSpeed.Value, slowTurnSpeed, percentToSlowTurnSpeed);
                
                Vector3 dirOnSlope = Vector3.ProjectOnPlane(currentVelocity.normalized,
                    effectiveGroundNormal).normalized;
                
                //If just became grounded, use the unprocessed velocity before collision
                //This prevents flying up slopes when landing and current velocity is toward slope's up
                if (!LastGroundingStatus.FoundAnyGround)
                    dirOnSlope = Vector3.ProjectOnPlane(previousVelocityOutput.normalized,
                        effectiveGroundNormal).normalized;

                //If ground is flat, just take flattened move input
                if (effectiveGroundNormal == Vector3.up)
                    moveInputOnSlope = moveInputCameraRelative.xoz().normalized;
                else if (Mathf.Approximately(MoveInput.Value.magnitude, 0f))
                    moveInputOnSlope = Vector3.zero;
                else
                    moveInputOnSlope = VectorUtils.FlattenDirectionOntoSlope(moveInputCameraRelative.xoz().normalized, effectiveGroundNormal);
                
                dir = Vector3.SmoothDamp(dirOnSlope, moveInputOnSlope,
                    ref dummyVel, currentTurnSpeed).normalized;

                dirOnSlopeGizmo = dirOnSlope;
            }
            else
            {
                currentTurnSpeed = TurnSpeedAir.Value;
                dir = Vector3.SmoothDamp(currentVelocity.xoz().normalized, moveInputCameraRelative.xoz().normalized,
                    ref dummyVel, currentTurnSpeed).normalized;
            }

            newDirection = dir;

            #endregion
            
            #region Get New Speed

            var steeringDir = moveInputCameraRelative.xoz().normalized;
            var steeringAngle = Vector3.Angle(dir, steeringDir);
            var steeringFac = steeringAngle / 180;

            //TurnFactor.Value = Vector3.SignedAngle(horizontalDir, steeringDir, Vector3.up) / 30;
            
            var rollingFriction = 6 * CoefficientOfRollingFriction.Value;//TODO: 6 was for medium size before
            var turningFriction = (1 + (CoefficientOfTurningFriction.Value * steeringFac));
            float friction = (GroundingStatus.FoundAnyGround) ?
                rollingFriction * turningFriction * Time.deltaTime : 0f;

            var drag = currentSpeed * DragCoefficientHorizontal.Value * Time.deltaTime;

            #region Accelerate to target speed

            float targetMagnitude;
            
            //If projected move input is in same direction as velocity, set target speed
            //Otherwise, assume player wants to stop and turn around 
            if (Vector3.Dot(dir.xoz(), moveInputOnSlope.xoz()) > 0f)
                targetMagnitude = moveInputOnSlope.magnitude;
            else
                targetMagnitude = 0f;
            
            var targetSpeed = BaseSpeed.Value * targetMagnitude;
            if (newSpeed < targetSpeed && GroundingStatus.FoundAnyGround)
            {
                var timeToTargetSpeed = 2f;    //Time to get to target speed from 0
                newSpeed = currentSpeed + targetSpeed * Time.deltaTime / timeToTargetSpeed;
                newSpeed -= friction;
            }
            else
            {
                //Only apply drag if not accelerating to base speed
                newSpeed = currentSpeed - friction - drag;
            }
            
            
            #endregion
            
            #endregion
            
            HorizontalSpeedOut.Value = newSpeed;
            storedDeflectVelocity = Vector3.zero;
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

            if (!GroundingStatus.FoundAnyGround)
            {
                float gravityAirborn = gravity * GravityFactorAirborn.Value;
                
                if (newVelocity.y <= 0f)  //Falling
                {
                    newVelocity.y += gravityAirborn * FallMultiplier.Value * Time.deltaTime;
                }
                else if (newVelocity.y > 0f)
                {
                    newVelocity.y += gravityAirborn * UpwardsGravityMultiplier.Value * Time.deltaTime;
                }
                
                var drag = newVelocity.y * DragCoefficientVertical.Value * Time.deltaTime;
                newVelocity.y -= drag;

                if (newVelocity.y < -Mathf.Abs(MaxFallSpeed.Value))   //Cap Speed
                {
                    newVelocity.y = -Mathf.Abs(MaxFallSpeed.Value);
                }
            }
            
            #endregion

            #region Grounded (slope)
            
            //Each frame, add gravity to velocity on slope
            if (GroundingStatus.FoundAnyGround)
            {
                if (newVelocity.y <= 0)  //Falling
                {
                    newVelocity.y = gravity * SlopeVerticalGravityFactor.Value * FallMultiplier.Value * Time.deltaTime;
                }
                else if (newVelocity.y > 0f) //Drag when moving up (Note: Affects going up slopes)
                {
                    var drag = -(newVelocity.y * newVelocity.y) * DragCoefficientVertical.Value * Time.deltaTime;
                    newVelocity.y = drag + gravity * UpwardsGravityMultiplier.Value * SlopeVerticalGravityFactor.Value * Time.deltaTime;
                }
                
                newVelocity = Vector3.ProjectOnPlane(newVelocity, effectiveGroundNormal);
            }

            #endregion

            return newVelocity;
        }
        
        protected override void UpdateRotation(Quaternion currentRotation)
        {
            Quaternion newRotation;
            Vector3 lookDirection = playerController.transform.forward;
            Vector3 velocityDirection = dirOnSlopeGizmo;
            
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
        
        private void CheckForDeflect(System.Object prevCollisionInfoObj, System.Object collisionInfoObj)
        {
            CollisionInfo collisionInfo = (CollisionInfo) collisionInfoObj;
            GameObject otherObj = collisionInfo.other.gameObject;
            
            //If going fast enough into the normal, deflect with knockback
        
            Vector3 velocityIntoNormal = Vector3.Project(previousVelocityOutput, -collisionInfo.contactNormal);
        
            float velocityGroundDot = Vector3.Dot(previousVelocityOutput.normalized, collisionInfo.contactNormal);

            if (EnableDeflect && velocityIntoNormal.magnitude >= DeflectThresholdVelocity.Value &&
                -velocityGroundDot > DeflectContactDotThreshold.Value)
            {
                //Reflect velocity in the XZ plane
                DeflectGameEvent.Raise();
                storedDeflectVelocity = Vector3.Reflect(NewVelocityOut.Value.normalized, collisionInfo.contactNormal);
                storedDeflectVelocity =
                    storedDeflectVelocity.xoz() * NewVelocityOut.Value.xoz().magnitude * DeflectFactor.Value;
            }
            
        }

        private void InitRollParticles()
        {
            //Init particles based on grounded or not
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;
            
            if (GroundingStatus.FoundAnyGround)
                PlayParticlesGameEvent.Raise();
            else
                StopParticlesGameEvent.Raise();
            
        }

        private void HandleUpdateParticles()
        {
            //Update particles when grounding status changes
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;
            
            //If became grounded
            if (GroundingStatus.FoundAnyGround && !LastGroundingStatus.FoundAnyGround)
                PlayParticlesGameEvent.Raise();
            //If became airborn
            else if (!GroundingStatus.FoundAnyGround && LastGroundingStatus.FoundAnyGround)
                StopParticlesGameEvent.Raise();
        }

        private void HandleRollSound()
        {
            if (!playerController.GroundingStatus.FoundAnyGround)
                return;

            float distBetweenSounds = 7f;
            float soundDelay = distBetweenSounds/velocityAlongSlope.magnitude;
            
            if (lastRollSoundTime + soundDelay < Time.time)
            {
                RollSoundGameEvent.Raise();
                lastRollSoundTime = Time.time;
            }
        }

        public override void DrawGizmos()
        {
            if (playerController == null) return;
        
            // set up all static parameters. these are used for all following Draw.Line calls
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.LineThicknessSpace = ThicknessSpace.Pixels;
            Draw.LineThickness = 6; // 4px wide

            float len = 10f;
            float radius = 0.25f;
            float alpha = .65f;
            Vector3 startPos = playerController.transform.position;

            var lines = new (Vector3 endPos, Color color)[]
            {
                (startPos + moveInputCameraRelative * (newSpeed / BaseSpeed.Value) * len,
                    false ? new Color(1f, 0f, 0f, .35f) : new Color(1f, 1f, 0f, alpha)),
                //(startPos + moveInputCameraRelative * len,
                //    new Color(1f, .3f, 0.3f, alpha)),
                //(startPos + newDirection * newSpeed,
                //    new Color(1f, 0.5f, 0, alpha)),
                (startPos + moveInputOnSlope * len,
                    new Color(1f, 0f, 1, alpha)),
                (startPos + dirOnSlopeGizmo * len,
                    new Color(.5f, .5f, 1f, alpha))
            };

            foreach (var line in lines)
            {
                Draw.Line(startPos, line.endPos, line.color);
                Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, line.endPos, radius, line.color);
            }
        }
    }
}