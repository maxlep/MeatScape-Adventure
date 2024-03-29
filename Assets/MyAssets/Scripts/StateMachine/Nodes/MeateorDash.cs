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


public class MeateorDash : BaseMovement
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private LayerMapper LayerMapper;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private TransformSceneReference slingshotTargetSceneReference;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private DynamicGameEvent PlayerCollidedWith;

    #endregion

    #region Outputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private Vector3Reference SlingshotDirection;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")][Required]
    private TriggerVariable MeateorCollideTrigger;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")] [Required]
    protected FloatReference KnockbackForceMagnitude;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")] [Required]
    protected Vector3Reference KnockbackForceOutput;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")] [Required]
    protected Vector3Reference MeateorStrikeHitPosition;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")] [Required]
    protected TimerReference MeateorStrikeDurationTimer;


    #endregion

    #region Velocity

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Velocity")][Required]
    private FloatReference MeateorStrikeMinSpeed;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Velocity")][Required]
    private FloatReference MeateorStrikeMaxSpeed;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Velocity")] [Required]
    protected LeanTweenType VelocityEasingType;

    #endregion
    
    #region Grounding

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Grounding")] [Required]
    private FloatReference GroundStickAngleInputDownwards;
        
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Grounding")] [Required]
    private FloatReference GroundStickAngleInputUpwards;
        
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Grounding")] [Required]
    private FloatReference GroundStickAngleOutput;

    #endregion

    #region Knockback

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Knockback")] [Required]
    protected bool EnableDeflect = true;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Knockback")] [Required] [ShowIf("$EnableDeflect")]
    protected FloatReference DeflectThresholdVelocity;
        
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Knockback")] [Required] [ShowIf("$EnableDeflect")]
    protected FloatReference DeflectContactDotThreshold;

    #endregion

    private float currentSpeed;
    private float duration;
    private Vector3 previousVelocityOutput = Vector3.zero;
    private Vector3 previousSlingshotDir = Vector3.zero;

    public override void Enter()
    {
        base.Enter();
        playerController.onStartUpdateVelocity += UpdateVelocity;
        playerController.onStartUpdateRotation += UpdateRotation;
        PlayerCollidedWith.Subscribe(CheckForHit);
        duration = MeateorStrikeDurationTimer.Duration;
        previousSlingshotDir = SlingshotDirection.Value;

        LeanTween.value(MeateorStrikeMaxSpeed.Value, MeateorStrikeMinSpeed.Value, duration)
            .setOnUpdate(speed =>
            {
                currentSpeed = speed;
            })
            .setEase(VelocityEasingType);
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

        Vector3 slingshotDirOnSlope = previousSlingshotDir;
        
        //If grounded, update slingshot direction
        if (motor.GroundingStatus.FoundAnyGround)
            slingshotDirOnSlope = VectorUtils.FlattenDirectionOntoSlope(SlingshotDirection.Value, effectiveGroundNormal);

        previousSlingshotDir = slingshotDirOnSlope;
        resultingVelocity = slingshotDirOnSlope * currentSpeed;

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
	
    protected override void UpdateRotation(Quaternion currentRotation)
    {
        NewRotationOut.Value = currentRotation;
    }

    public override void Exit()
    {
        base.Exit();
        playerController.onStartUpdateVelocity -= UpdateVelocity;
        playerController.onStartUpdateRotation -= UpdateRotation;
        PlayerCollidedWith.Unsubscribe(CheckForHit);
        

    }

    private void CheckForHit(System.Object prevCollisionInfoObj, System.Object collisionInfoObj)
    {
        CollisionInfo collisionInfo = (CollisionInfo) collisionInfoObj;
        GameObject otherObj = collisionInfo.other.gameObject;
        
        if (otherObj.layer == LayerMapper.GetLayer(LayerEnum.Enemy))
        {
            playerController.UngroundMotor();
            Vector3 playerToTarget = (otherObj.transform.position.xoz() - playerController.transform.position.xoz()).normalized;
            //Vector3 knockbackDir = Vector3.RotateTowards(-playerToTarget, Vector3.up, 30f, 0f);
            KnockbackForceOutput.Value = -NewVelocityOut.Value.normalized.xoz() * KnockbackForceMagnitude.Value + 
                                         Vector3.up * KnockbackForceMagnitude.Value;

            MeateorStrikeHitPosition.Value = collisionInfo.contactPoint;
            MeateorCollideTrigger.Activate();
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
            }
        }
    }
}
