using KinematicCharacterController;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
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
        protected FloatReference AirForwardForce;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Horizontal")] [Required]
        protected Vector3Reference PreviousVelocity;

        #endregion

        #region Vertical Movement

        [HideIf("$collapsed")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical")]
        private FloatReference FallMultiplier;

        [HideIf("$collapsed")]
        [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        [TabGroup("Vertical")]
        private FloatReference MaxFallSpeed;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference DragCoefficientVertical;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
        [TabGroup("Vertical")] [Required]
        protected FloatReference GravityFactor;
        
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
        private FloatReference GroundStickAngleInput;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        [TabGroup("Grounding")] [Required]
        private FloatReference GroundStickAngleOutput;

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
        protected GameEvent RollSoundGameEvent;


        #endregion

        private Vector3 velocityAlongSlope;
        private Vector3 moveInputOnSlope;
        private Vector3 previousVelocityOutput = Vector3.zero;

        private float lastRollSoundTime;


        #region Lifecycle methods

        public override void Enter()
        {
            base.Enter();
            
            //If airborn, give forward boost
            if (!playerController.GroundingStatus.FoundAnyGround)
            {
                Vector3 impulseDir = (playerController.transform.forward + playerController.transform.up).normalized;
                playerController.AddImpulse(impulseDir * AirForwardForce.Value);
            }
                

            lastRollSoundTime = Mathf.NegativeInfinity;
            GroundStickAngleOutput.Value = GroundStickAngleInput.Value;
            InitRollParticles();
            gravity *= GravityFactor.Value;
        }

        public override void Exit()
        {
            base.Exit();
            StopParticlesGameEvent.Raise();
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
            Vector3 horizontalVelocity = CalculateHorizontalVelocity(currentVelocity);
            Vector3 verticalVelocity = CalculateVerticalVelocity(currentVelocity);
            
            //Redirect impulseVelocityRedirectble if conditions met
            if (EnableRedirect && CheckRedirectConditions(impulseVelocityRedirectble))
                totalImpulse += CalculateRedirectedImpulse(impulseVelocityRedirectble);
            else
                totalImpulse += impulseVelocityRedirectble;
            
            resultingVelocity = horizontalVelocity + verticalVelocity;
            resultingVelocity += totalImpulse;
            
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

            
            #region Bounce

            //Bounce if just became grounded
            Vector3 velocityIntoGround = Vector3.Project(PreviousVelocity.Value, -GroundingStatus.GroundNormal);
            
            float velocityGroundDot = Vector3.Dot(PreviousVelocity.Value.normalized, GroundingStatus.GroundNormal);

            if (!LastGroundingStatus.FoundAnyGround && GroundingStatus.FoundAnyGround &&
                velocityIntoGround.magnitude >= BounceThresholdVelocity.Value &&
                -velocityGroundDot > BounceGroundDotThreshold.Value)
            {
                playerController.UngroundMotor();
                BounceGameEvent.Raise();

                //Negative dot of velocity and ground normal
                //Negative to get in range of 0 -> 1 where 1 means directly into ground
                float bounceFactorNormalMultiplier = -velocityGroundDot;
                
                //Lerp from normal bounce to half based on ground normal velocity dot
                float normalBounceFactorMultiplier = Mathf.Lerp(BounceFactor.Value, BounceFactor.Value * 2f, bounceFactorNormalMultiplier);
                
                //Reflect velocity perfectly then dampen the y based on dot with normal
                Vector3 reflectedVelocity = Vector3.Reflect(PreviousVelocity.Value, GroundingStatus.GroundNormal);
                reflectedVelocity.y *= normalBounceFactorMultiplier;
                
                //Redirect bounce if conditions met
                if (EnableRedirect && CheckRedirectConditions(reflectedVelocity))
                {
                    reflectedVelocity = CalculateRedirectedImpulse(reflectedVelocity);
                }
                    

                resultingVelocity = normalBounceFactorMultiplier * reflectedVelocity;
            }

            #endregion
            
            previousVelocityOutput = resultingVelocity;
            
            return resultingVelocity;
        }

        private Vector3 dirOnSlopeGizmo;

        private Vector3 CalculateHorizontalVelocity(Vector3 currentVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            
            velocityAlongSlope = Vector3.ProjectOnPlane(currentVelocity, GroundingStatus.GroundNormal);
            float currentSpeed = (GroundingStatus.FoundAnyGround)
                ? velocityAlongSlope.magnitude
                : currentVelocity.xoz().magnitude;

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
                        GroundingStatus.GroundNormal).normalized;

                //If ground is flat, just take flattened move input
                if (GroundingStatus.GroundNormal == Vector3.up)
                    moveInputOnSlope = moveInputCameraRelative.xoz().normalized;
                else if (Mathf.Approximately(MoveInput.Value.magnitude, 0f))
                    moveInputOnSlope = Vector3.zero;
                else
                    moveInputOnSlope = FlattenMoveInputOntoSlope(MoveInput.Value, GroundingStatus.GroundNormal);
                
                dir = Vector3.SmoothDamp(dirOnSlope, moveInputOnSlope,
                    ref dummyVel, currentTurnSpeed).normalized;

                dirOnSlopeGizmo = dirOnSlope;
            }
            else
            {
                currentTurnSpeed = TurnSpeedAir.Value;
                dir = Vector3.SmoothDamp(currentVelocity.xoz().normalized, moveInputCameraRelative.normalized,
                    ref dummyVel, currentTurnSpeed).normalized;
            }

            newDirection = dir;

            #endregion
            
            #region Get New Speed

            var steeringDir = moveInputCameraRelative;
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
            if (Vector3.Dot(dir, moveInputOnSlope) > 0f)
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
            return newDirection * newSpeed;
        }

        private Vector3 CalculateVerticalVelocity(Vector3 currentVelocity)
        {
            CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
            CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;

            //Return if standing on flat ground
            if (GroundingStatus.FoundAnyGround && GroundingStatus.GroundNormal == Vector3.up)
                return Vector3.zero;

            #region Airborn

            Vector3 newVelocity = currentVelocity.y * Vector3.up;

            if (!GroundingStatus.FoundAnyGround)
            {
                if (newVelocity.y <= 0)  //Falling
                {
                    newVelocity.y += gravity * (FallMultiplier.Value - 1) * Time.deltaTime;
                }
                else if (newVelocity.y > 0f) //Drag when moving up (Note: Affects going up slopes)
                {
                    var drag = -(newVelocity.y * newVelocity.y) * DragCoefficientVertical.Value * Time.deltaTime;
                    newVelocity.y += drag + gravity * Time.deltaTime;
                }
                else
                {
                    newVelocity.y += gravity * Time.deltaTime;
                }
            
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
                newVelocity.y = gravity * SlopeVerticalGravityFactor.Value * Time.deltaTime;
                newVelocity = Vector3.ProjectOnPlane(newVelocity, GroundingStatus.GroundNormal);
            }

            #endregion

            return newVelocity;
        }
        
        protected override void UpdateRotation(Quaternion currentRotation)
        {
            Quaternion newRotation;
            Vector3 lookDirection = playerController.transform.forward;
            Vector3 velocityDirection = NewVelocityOut.Value.normalized;

            //This rotation will keep the collider up along round normal
            newRotation = quaternion.LookRotation(velocityDirection, playerController.GroundingStatus.GroundNormal);

            NewRotationOut.Value = newRotation;
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