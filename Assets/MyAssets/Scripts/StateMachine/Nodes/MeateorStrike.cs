using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using MyAssets.Graphs.StateMachine.Nodes;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;


public class MeateorStrike : RollMovement
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private LayerMapper LayerMapper;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMask EnemyMask;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMask InteractableMask;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private Vector3Reference SlingshotDirection;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private TransformSceneReference slingshotTargetSceneReference;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected FloatReference KnockbackForceMagnitude;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected FloatReference KnockbackAngleAboveHorizontal;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private TransformSceneReference currentTargetSceneReference;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")][Required]
    private FloatValueReference MeateorStrikeMinSpeed;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")][Required]
    private FloatValueReference MeateorStrikeMaxSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")][Required]
    private FloatValueReference MeteorStrikeDashForce;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected LeanTweenType VelocityEasingType;

    #endregion

    #region Outputs
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")][Required]
    private TriggerVariable MeateorCollideTrigger;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")] [Required]
    protected Vector3Reference KnockbackForceOutput;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")] [Required]
    protected Vector3Reference MeateorStrikeHitPosition;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")] [Required]
    protected TimerReference MeateorStrikeDurationTimer;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")][Required]
    private GameEvent MeateorStrikeHitEvent;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private DynamicGameEvent MeateorStrikeCollision;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private BoolReference IsHomingMeateorStrike;

    #endregion

    private float currentSpeed;
    private float duration;
    private bool isHoming;
    private Vector3 previousSlingshotDir = Vector3.zero;

    public override void Enter()
    {
        base.Enter();
        
        PlayerCollidedWith.Subscribe(CheckForHit);
        
        duration = MeateorStrikeDurationTimer.Duration;
        previousSlingshotDir = SlingshotDirection.Value;

        LeanTween.value(MeateorStrikeMaxSpeed.Value, MeateorStrikeMinSpeed.Value, duration)
            .setOnUpdate(speed =>
            {
                currentSpeed = speed;
            })
            .setEase(VelocityEasingType);

        
        //If null homing target, check current lock on target (null if coming from meateor dash)
        //TODO: this essential bypasses the dot product calculations for slingshotTarget in slingshot state.
        if (currentTargetSceneReference.Value != null)
            slingshotTargetSceneReference.Value = currentTargetSceneReference.Value;
        
        //If no target, dont use homing logic
        if (slingshotTargetSceneReference.Value == null)
            isHoming = false;
        
        //Add impulse for dash if not homing
        if (!isHoming)
            playerController.AddImpulse(SlingshotDirection.Value * MeteorStrikeDashForce.Value);
    }
    
    public override void Exit()
    {
        base.Exit();
        PlayerCollidedWith.Unsubscribe(CheckForHit);
    }

    public override void Execute()
    {
        base.Execute();
        IsHomingMeateorStrike.Value = isHoming;
    }

    protected override Vector3 CalculateVelocity(VelocityInfo velocityInfo)
    {
        Vector3 currentVelocity = velocityInfo.currentVelocity;
        Vector3 impulseVelocity = velocityInfo.impulseVelocity;
        Vector3 impulseVelocityRedirectble = velocityInfo.impulseVelocityRedirectble;
        
        Vector3 totalImpulse = impulseVelocity;
        Vector3 resultingVelocity = Vector3.zero;
        
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

        #region Calculate New Velocity
        
        //Use roll movement logic if not homing
        if (!isHoming)
        {
            Vector3 horizontalVelocity = CalculateHorizontalVelocity(currentVelocity, effectiveGroundNormal);
            Vector3 verticalVelocity = Vector3.zero;
            
            //Only apply gravity if grounded
            if (motor.GroundingStatus.FoundAnyGround)
                verticalVelocity = CalculateVerticalVelocity(currentVelocity, effectiveGroundNormal);

            resultingVelocity = horizontalVelocity + verticalVelocity;
            resultingVelocity += totalImpulse;
        }
        //Else use homing logic
        else
        {
            Vector3 homingVelocity = CalculateHomingVelocity(currentVelocity, effectiveGroundNormal);

            resultingVelocity = homingVelocity;
            resultingVelocity += totalImpulse;
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

    private Vector3 CalculateHomingVelocity(Vector3 currentVelocity, Vector3 effectiveGroundNormal)
    {
        Vector3 newVelocity = Vector3.zero;
        KinematicCharacterMotor motor = playerController.CharacterMotor;
        Vector3 slingshotDirOnSlope = previousSlingshotDir;
        
        //If grounded, update slingshot direction
        if (motor.GroundingStatus.FoundAnyGround)
            slingshotDirOnSlope = VectorUtils.FlattenDirectionOntoSlope(SlingshotDirection.Value, effectiveGroundNormal);

        previousSlingshotDir = slingshotDirOnSlope;
        newVelocity = slingshotDirOnSlope * currentSpeed;
        
        //If is homing mode and has target, override direction
        if (isHoming && slingshotTargetSceneReference.Value != null)
        {
            Vector3 playerToTarget =
                (slingshotTargetSceneReference.Value.position - playerController.transform.position).normalized;
            newVelocity = playerToTarget * currentSpeed;
        }

        return newVelocity;
    }
	
    protected override void UpdateRotation(Quaternion currentRotation)
    {
        //If not homing, use roll movement logic
        if (!isHoming)
        {
            base.UpdateRotation(currentRotation);
            return;
        }
        
        NewRotationOut.Value = currentRotation;
    }

    private void CheckForHit(System.Object prevCollisionInfoObj, System.Object collisionInfoObj)
    {
        CollisionInfo collisionInfo = (CollisionInfo) collisionInfoObj;
        GameObject otherObj = collisionInfo.other.gameObject;
        
        if (otherObj.IsInLayerMask(EnemyMask) ||
            otherObj.IsInLayerMask(InteractableMask))
        {
            playerController.UngroundMotor();
            
            //Get direction by taking negative vel direction and turning it upwards towards world up (based on above horizontal angle)
            Vector3 knockbackDir = Vector3.RotateTowards(-NewVelocityOut.Value.normalized.xoz(), Vector3.up,
                KnockbackAngleAboveHorizontal.Value * Mathf.Deg2Rad, 0f);

            KnockbackForceOutput.Value = knockbackDir * KnockbackForceMagnitude.Value;

            MeateorStrikeHitPosition.Value = collisionInfo.contactPoint;
            MeateorCollideTrigger.Activate();
            MeateorStrikeCollision.Raise(collisionInfo);
            MeateorStrikeHitEvent.Raise();
        }
        else
        {
            //If going fast enough into the normal, deflect with knockback
            Vector3 velocityIntoNormal = Vector3.Project(previousVelocityOutput, -collisionInfo.contactNormal);
            
            float velocityGroundDot = Vector3.Dot(previousVelocityOutput.normalized, collisionInfo.contactNormal);

            if (EnableDeflect && velocityIntoNormal.magnitude >= DeflectThresholdVelocity.Value &&
                -velocityGroundDot > DeflectContactDotThreshold.Value)
            {
                playerController.UngroundMotor();
                KnockbackForceOutput.Value = -NewVelocityOut.Value.normalized.xoz() * KnockbackForceMagnitude.Value + 
                                             Vector3.up * KnockbackForceMagnitude.Value;
                MeateorStrikeHitPosition.Value = collisionInfo.contactPoint;
                MeateorCollideTrigger.Activate();
                MeateorStrikeHitEvent.Raise();
            }
        }
    }

    //To be called by the transition into this state
    public void SetIsHoming(bool homingEnabled)
    {
        isHoming = homingEnabled;
    }
}
