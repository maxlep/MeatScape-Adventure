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

public class Bounce : BaseMovement
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    protected Vector3Reference PreviousVelocityDuringUpdate;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected FloatReference BounceFactor;
        
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected FloatReference BounceThresholdVelocity;
        
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    protected FloatReference BounceGroundDotThreshold;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    protected FloatValueReference GroundSlamVelocity;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    protected FloatValueReference BounceHorizontalVelocity;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    protected FloatValueReference BounceVerticalVelocity;

    #endregion

    #region Game Events

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Events")] [Required]
    protected GameEvent BounceGameEvent;

    #endregion
    
    public override void Enter()
    {
        base.Enter();
        DoBounce();
    }

    private void DoBounce()
    {
        CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
        CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;
        
        //Don't allow less than ground slam velocity
        //Sometimes prev velocity will be frame after collision and y is positive so wont bounce properly
        Vector3 prevVel = PreviousVelocityDuringUpdate.Value;
        prevVel.y = Mathf.Min(prevVel.y, -GroundSlamVelocity.Value);
        
        float velocityGroundDot = Vector3.Dot(prevVel.normalized, GroundingStatus.GroundNormal);

        //NOTE: Extended this from roll movement just to add !JumpPressed.Value in this if statement to prevent roll bounce before transition to flatten
        //If roll pressed, dont bounce, instead will want to slingshot
        if (-prevVel.y >= BounceThresholdVelocity.Value &&
            -velocityGroundDot > BounceGroundDotThreshold.Value)
        {
            playerController.UngroundMotor();
            BounceGameEvent.Raise();

            //Reflect velocity perfectly then dampen the y based on dot with normal
            Vector3 reflectedVelocity = Vector3.Reflect(prevVel, GroundingStatus.GroundNormal);
            //reflectedVelocity.y *= BounceFactor.Value;
            
                
            //Redirect bounce if conditions met
            if (EnableRedirect && CheckRedirectConditions(reflectedVelocity))
                reflectedVelocity = CalculateRedirectedImpulse(reflectedVelocity);


            reflectedVelocity.y = BounceVerticalVelocity.Value;
            
            //Cap horizontal velocity
            if (reflectedVelocity.xoz().magnitude > BounceHorizontalVelocity.Value)
            {
                Vector3 horizontalDir = reflectedVelocity.xoz().normalized;
                reflectedVelocity.x = 0f;
                reflectedVelocity.z = 0f;
                reflectedVelocity += horizontalDir * BounceHorizontalVelocity.Value;
            }
            

            playerController.SetVelocity(reflectedVelocity, true);
        }
    }
}
