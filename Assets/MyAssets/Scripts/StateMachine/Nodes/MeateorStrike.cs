using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;


public class MeateorStrike : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected Vector3Reference NewVelocityOut;
	
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected QuaternionReference NewRotationOut;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [Required]
    private Vector3Reference SlingshotDirection;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [Required]
    private FloatReference MeateorStrikeSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [Required]
    private FloatReference MeateorStrikeMinSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [Required]
    private FloatReference MeateorStrikeMaxSpeed;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [Required]
    private TransformSceneReference slingshotTargetSceneReference;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [Required]
    private DynamicGameEvent PlayerCollidedWith;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [Required]
    private LayerMapper LayerMapper;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [Required]
    private TriggerVariable MeateorCollideTriger;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected FloatReference KnockbackForceMagnitude;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected Vector3Reference KnockbackForceOutput;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected Vector3Reference MeateorStrikeHitPosition;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected TimerReference MeateorStrikeDurationTimer;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Outputs")] [Required]
    protected LeanTweenType VelocityEasingType;


    private float currentSpeed;
    private float duration;

    public override void Enter()
    {
        base.Enter();
        playerController.onStartUpdateVelocity += UpdateVelocity;
        playerController.onStartUpdateRotation += UpdateRotation;
        PlayerCollidedWith.Subscribe(CheckForHit);
        duration = MeateorStrikeDurationTimer.Duration;

        LeanTween.value(MeateorStrikeMaxSpeed.Value, MeateorStrikeMinSpeed.Value, duration)
            .setOnUpdate(speed =>
            {
                currentSpeed = speed;
            })
            .setEase(VelocityEasingType);
    }

    private void UpdateVelocity(VelocityInfo velocityInfo)
    {
        NewVelocityOut.Value = SlingshotDirection.Value * currentSpeed;
        
        if (slingshotTargetSceneReference.Value != null)
        {
            Vector3 playerToTarget =
                (slingshotTargetSceneReference.Value.position - playerController.transform.position).normalized;
            NewVelocityOut.Value = playerToTarget * currentSpeed;
        }
    }
	
    private void UpdateRotation(Quaternion currentRotation)
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
            MeateorCollideTriger.Activate();
        }
        else
        {
            //TODO: check if going fast enough into the normal, if so, knockback
        }
    }
}
