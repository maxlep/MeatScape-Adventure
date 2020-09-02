using System.Linq;
using System.Collections.Generic;
using KinematicCharacterController;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor charMotor;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTrans;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform root;
    [SerializeField] private PlayerSize startSize;
    
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private Vector3Reference NewVelocity;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private QuaternionReference NewRotation;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference StoredJumpVelocity;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector2Reference MoveInput;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference JumpPressed;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private BoolReference IsGrounded;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable AttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable JumpTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable DownwardAttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable GroundPoundTrigger;

    [SerializeField] private VariableContainer SizeProps;
    [FoldoutGroup("Size Controlled Properties")] [SerializeField] private FloatVariable MaxJumpHeight;

    private Vector3 moveDirection;
    private InputAction playerMove;

    private Dictionary<PlayerSize, string> PlayerSizeMap;
    private Dictionary<string, float> SizePropsDict;
    private PlayerSize _currentSize;

    public PlayerSize CurrentSize { get => _currentSize; set => SetPlayerSize(value); }

    public Transform GetCameraTrans() => cameraTrans;
    public Transform GetFirePoint() => firePoint;
    public Animator GetAnimator() => animator;
    public Transform GetRoot() => root;

    public delegate void _OnStartUpdateVelocity(Vector3 currentVelocity);
    public delegate void _OnStartUpdateRotation(Quaternion currentRotation);
    public event _OnStartUpdateVelocity onStartUpdateVelocity;
    public event _OnStartUpdateRotation onStartUpdateRotation;

    #region Unity Methods

    private void Start()
    {
        charMotor.CharacterController = this;
    }

    void Awake()
    {
        playerMove = InputManager.Instance.GetPlayerMove_Action();
        InputManager.Instance.onJump_Pressed += () => JumpPressed.Value = true;
        InputManager.Instance.onJump_Released += () => JumpPressed.Value = false;

        InputManager.Instance.onJump_Pressed += () => JumpTrigger.Activate();
        InputManager.Instance.onAttack += () => AttackTrigger.Activate();
        InputManager.Instance.onDownwardAttack += () => DownwardAttackTrigger.Activate();
        InputManager.Instance.onGroundPound += () => GroundPoundTrigger.Activate();
        InputManager.Instance.onRegenerateMeat += () => CurrentSize++;

        PlayerSizeMap = new Dictionary<PlayerSize, string> {
            {PlayerSize.Small, "Small"},
            {PlayerSize.Medium, "Medium"},
            {PlayerSize.Large, "Large"}
        };
        SizePropsDict = SizeProps.GetFloatVariables().ToDictionary(v => v.name, v => v.Value);

        CurrentSize = startSize;
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
        if (onStartUpdateRotation != null) onStartUpdateRotation.Invoke(currentRotation);
        currentRotation = NewRotation.Value;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (onStartUpdateVelocity != null) onStartUpdateVelocity.Invoke(currentVelocity);

        currentVelocity = NewVelocity.Value;

        if (!Mathf.Approximately(StoredJumpVelocity.Value, 0f))
        {
            currentVelocity.y = StoredJumpVelocity.Value;
        }
        
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // This is called after the motor has finished everything in its update
        StoredJumpVelocity.Value = 0f;
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
        IsGrounded.Value = MaintainingGround();

        // animator.SetFloat("HorizontalSpeed", Mathf.Sqrt(Mathf.Pow(NewVelocity.Value.x, 2) + Mathf.Pow(NewVelocity.Value.z, 2)));
        // animator.SetFloat("VerticalVelocity", NewVelocity.Value.y);
        // animator.SetBool("IsGrounded", IsGrounded.Value);
    }

    
    public bool MaintainingGround()
    {
        return charMotor.GroundingStatus.IsStableOnGround;
    }

    private void GetInput()
    {
        MoveInput.Value = playerMove.ReadValue<Vector2>();
    }

    public void UngroundMotor()
    {
        IsGrounded.Value = false;
        charMotor.ForceUnground(0.1f);
    }
    
    private void SetPlayerSize(PlayerSize value) {
        if(value < PlayerSize.Small) _currentSize = PlayerSize.Small;
        else if(value > PlayerSize.Large) _currentSize = PlayerSize.Large;
        else _currentSize = value;
        Debug.Log(value);
        MaxJumpHeight.Value = SizePropsDict[$"{PlayerSizeMap[_currentSize]}JumpHeight"];
    }
    
}

public enum PlayerSize {
    Small,
    Medium,
    Large
}
