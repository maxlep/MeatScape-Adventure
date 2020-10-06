using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class SlopeSliding : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    private Vector3Reference NewVelocityOut;
    
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    private QuaternionReference NewRotationOut;
    
    [HideIf("$zoom")] [LabelWidth(165)] [SerializeField] [Required]
    private TransformSceneReference PlayerCameraTransform;
    
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
    private Vector2Reference MoveInput;
    
    private Vector3 moveDirection;
    
    public override void Enter()
    {
        base.Enter();
        playerController.onStartUpdateVelocity += UpdateVelocity;
        playerController.onStartUpdateRotation += UpdateRotation;
        SetMoveDirection(); //Call to force update moveDir in case updateRot called b4 updateVel
    }
    
    private void UpdateVelocity(Vector3 currentVelocity)
    {
        SetMoveDirection();
        
        Vector3 newVelocity;
        CharacterGroundingReport GroundingStatus = playerController.GroundingStatus;
        CharacterTransientGroundingReport LastGroundingStatus = playerController.LastGroundingStatus;
        
        //Take move input direction directly and flatten (Dont do turn smoothing for now)
        float slopeMoveSpeed = 10f;
        newVelocity = moveDirection.Flatten() * slopeMoveSpeed;
        
        //Project velocity sideways along slope
        Vector3 slopeRight = Vector3.Cross(Vector3.up, GroundingStatus.GroundNormal);
        Vector3 slopeOut = Vector3.Cross(slopeRight, Vector3.up);
        newVelocity = Vector3.ProjectOnPlane(newVelocity, slopeOut).Flatten();
        
        //Add velocity down slope
        float slopeFallSpeed = 10f;
        Vector3 fallVelocity = slopeFallSpeed * Vector3.down;
        newVelocity += Vector3.ProjectOnPlane(fallVelocity, GroundingStatus.GroundNormal);

        NewVelocityOut.Value = newVelocity;
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
}
