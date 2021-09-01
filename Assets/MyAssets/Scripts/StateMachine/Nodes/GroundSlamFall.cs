using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using MyAssets.Graphs.StateMachine.Nodes;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class GroundSlamFall : RollMovement
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    protected BoolReference JumpPressed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    protected Vector3Reference VelocityBeforeGroundSlam;

    private bool bounced;

    public override void Enter()
    {
        base.Enter();
        bounced = false;
    }

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
            
            float velocityGroundDot = Vector3.Dot(previousVelocityOutput.normalized, GroundingStatus.GroundNormal);

            //NOTE: Extended this from roll movement just to add !JumpPressed.Value in this if statement to prevent roll bounce before transition to flatten
            //If roll pressed, dont bounce, instead will want to slingshot
            if (EnableBounce &&
                !RollInputPressed.Value &&
                !LastGroundingStatus.FoundAnyGround &&
                GroundingStatus.FoundAnyGround &&
                -previousVelocityOutput.y >= BounceThresholdVelocity.Value &&
                -velocityGroundDot > BounceGroundDotThreshold.Value &&
                !JumpPressed.Value)
            {
                playerController.UngroundMotor();
                BounceGameEvent.Raise();

                //Reflect velocity perfectly then dampen the y based on dot with normal
                Vector3 reflectedVelocity = Vector3.Reflect(previousVelocityOutput, GroundingStatus.GroundNormal);
                reflectedVelocity.y *= BounceFactor.Value;
                
                //Redirect bounce if conditions met
                if (EnableRedirect && CheckRedirectConditions(reflectedVelocity))
                    reflectedVelocity = CalculateRedirectedImpulse(reflectedVelocity);


                if (reflectedVelocity.y < MinYBounceVelocity.Value)
                    reflectedVelocity.y = MinYBounceVelocity.Value;

                bounced = true;
                resultingVelocity = reflectedVelocity;
            }

            #endregion
            
            #region Ground Stick Angle

            if (resultingVelocity.y >= 0)
                GroundStickAngleOutput.Value = GroundStickAngleInputUpwards.Value;
            else
                GroundStickAngleOutput.Value = GroundStickAngleInputDownwards.Value;

            #endregion

            //Update while still falling
            if (!bounced && !GroundingStatus.FoundAnyGround && resultingVelocity.y < 0f)
                VelocityBeforeGroundSlam.Value = resultingVelocity;
            
            previousVelocityOutput = resultingVelocity;
            return resultingVelocity;
        }
}
