using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using KinematicCharacterController;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor charMotor;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private StateMachineParameters parameters;
    [SerializeField] private Transform characterTrans;
    [SerializeField] private Transform cameraTrans;
    
    public Vector2 moveInput { get; private set; }
    public bool jumpPressed { get; private set; } = false;

    private Vector3 playerVelocity = Vector3.zero;
    private Vector3 moveDirection;
    private Vector3 internalVelocityAdd = Vector3.zero;
    private InputAction playerMove;
    private float jumpVelocity = 0f;

    public Transform GetCameraTrans() => cameraTrans;
    
    public void SetPlayerVelocity(Vector3 newVelocity) => playerVelocity = newVelocity;
    public void SetJumpVelocity(float newVelocity) => jumpVelocity = newVelocity;
    public void UngroundMotor() => charMotor.ForceUnground(0.1f);

    public delegate void _OnStartUpdateVelocity(Vector3 currentVelocity);
    public event _OnStartUpdateVelocity onStartUpdateVelocity;

    #region Unity Methods

    private void Start()
    {
        charMotor.CharacterController = this;
    }

    void Awake()
    {
        playerMove = InputManager.Instance.GetPlayerMove_Action();
        InputManager.Instance.onJump_Pressed += () => jumpPressed = true;
        InputManager.Instance.onJump_Released += () =>
        {
            jumpPressed = false;
        };
        InputManager.Instance.onJump_Pressed += () => stateMachine.ActivateTrigger("Jump");
        InputManager.Instance.onStab += () => stateMachine.ActivateTrigger("Attack");
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateParameters();
    }

    #endregion

    #region CharacterController Methods

    public void BeforeCharacterUpdate(float deltaTime)
    {
        // This is called before the motor does anything
        GetInput();
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // This is called when the motor wants to know what its rotation should be right now
        if (moveDirection.AlmostZero()) return;
        currentRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (onStartUpdateVelocity != null) onStartUpdateVelocity.Invoke(currentVelocity);

        currentVelocity = playerVelocity;

        if (!Mathf.Approximately(jumpVelocity, 0f))
        {
            currentVelocity.y = jumpVelocity;
        }
        
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // This is called after the motor has finished everything in its update
        jumpVelocity = 0f;
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        // This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        // This is called when the motor's ground probing detects a ground hit
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        // This is called when the motor's movement logic detects a hit
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        // This is called after every hit detected in the motor, to give you a chance to modify the HitStabilityReport any way you want
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        // This is called by the motor when it is detecting a collision that did not result from a "movement hit".
    }

    #endregion

    private void UpdateParameters()
    {
        parameters.SetBool("IsGrounded", MaintainingGround());
    }

    
    public bool MaintainingGround()
    {
        return charMotor.GroundingStatus.IsStableOnGround;
    }

    private void GetInput()
    {
        moveInput = playerMove.ReadValue<Vector2>();
    }
    
    
}
