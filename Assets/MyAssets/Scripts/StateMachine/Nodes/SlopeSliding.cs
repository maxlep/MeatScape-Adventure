using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class SlopeSliding : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    private Vector3Reference NewVelocityOut;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    private QuaternionReference NewRotationOut;
    
    [HideIf("$collapsed")] [LabelWidth(165)] [SerializeField] [Required]
    private TransformSceneReference PlayerCameraTransform;
    
    [HideIf("$collapsed")] [LabelWidth(165)] [SerializeField] [Required]
    private TransformSceneReference PlayerSlopeSlidingPivot;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    private Vector2Reference MoveInput;
    
    private Vector3 moveDirection;
    
    public override void Enter()
    {
        base.Enter();
        playerController.onStartUpdateVelocity += UpdateVelocity;
        playerController.onStartUpdateRotation += UpdateRotation;
        SetMoveDirection(); //Call to force update moveDir in case updateRot called b4 updateVel
    }

    public override void Exit()
    {
        base.Exit();
        playerController.onStartUpdateVelocity -= UpdateVelocity;
        playerController.onStartUpdateRotation -= UpdateRotation;
        PlayerSlopeSlidingPivot.Value.localPosition = Vector3.zero;
        PlayerSlopeSlidingPivot.Value.localRotation = Quaternion.identity;
    }

    private void UpdateVelocity(VelocityInfo velocityInfo)
    {
        SetMoveDirection();
        
        Vector3 newVelocity;
        CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
        CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;
        
        //Take move input direction directly and flatten (Dont do turn smoothing for now)
        float slopeMoveSpeed = 10f;
        newVelocity = moveDirection.xoz() * slopeMoveSpeed;
        
        //Project velocity sideways along slope
        Vector3 slopeRight = Vector3.Cross(Vector3.up, GroundingStatus.GroundNormal);
        Vector3 slopeOut = Vector3.Cross(slopeRight, Vector3.up);
        newVelocity = Vector3.ProjectOnPlane(newVelocity, slopeOut).xoz();
        
        //Add velocity down slope
        float slopeFallSpeed = 20f;
        Vector3 fallVelocity = slopeFallSpeed * Vector3.down;
        newVelocity += Vector3.ProjectOnPlane(fallVelocity, GroundingStatus.GroundNormal);
        
        NewVelocityOut.Value = newVelocity;
        UpdateSlopeSlidePivot();
    }
    
    private void UpdateRotation(Quaternion currentRotation)
    {
        NewRotationOut.Value = currentRotation;
    }
    
    private void SetMoveDirection()
    {
        Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        moveDirection = new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }
    
    private void UpdateSlopeSlidePivot()
    {
        if (!Mathf.Approximately(NewVelocityOut.Value.magnitude, 0f) &&
            !Mathf.Approximately(playerController.GroundingStatus.GroundNormal.magnitude, 0f) )
        {
            PlayerSlopeSlidingPivot.Value.rotation =
                Quaternion.LookRotation(NewVelocityOut.Value.normalized, 
                    playerController.GroundingStatus.GroundNormal);
        }
    }
}
