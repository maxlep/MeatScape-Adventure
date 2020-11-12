using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using KinematicCharacterController;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Player;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public interface testInterface
{
    int someInt();
}

public class PlayerController : SerializedMonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor charMotor;
    [SerializeField] private LayerMapper layerMapper;
    public CapsuleCollider Collider;
    [SerializeField] private AudioClip jumpAttackClip;
    [SerializeField] private AimTargetter aimTargetter;
    [SerializeField] private Transform meatClumpContainer;

    [FoldoutGroup("Referenced Inputs")] [SerializeField] private Vector3Reference NewVelocity;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private QuaternionReference NewRotation;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference StoredJumpVelocity;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference MaxSlopeSlideAngle;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector2Reference MoveInput;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector3Reference BaseVelocity;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference JumpPressed;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference IsSlopeSlideValid;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private TimerReference SlopeSlideTimer;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private BoolReference IsGrounded;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private BoolReference IsOnSlidebleSlope;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private IntReference PlayerCurrentSize;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable AttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable JumpTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable DownwardAttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable GroundPoundTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable JumpAttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable CallMeatClump;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable AddImpulseTrigger;

    [FoldoutGroup("Interaction Parameters")] [SerializeField] private FloatReference ClumpThrowKnockbackSpeed;
    [FoldoutGroup("Interaction Parameters")] [SerializeField] private FloatReference ClumpReturnKnockbackSpeed;

    [FoldoutGroup("Size Parameters")] [SerializeField] private TransformSceneReference sizeChangePivot;
    [FoldoutGroup("Size Parameters")] [SerializeField] private PlayerSize startSize;
    [FoldoutGroup("Size Parameters")] public bool freezeSize;
    [FoldoutGroup("Size Parameters")] public bool unlimitedClumps;
    [FoldoutGroup("Size Parameters")] [HideReferenceObjectPicker] [SerializeField] [SuffixLabel("ms")]
    private FloatReference changeSizeTime;
    [FoldoutGroup("Size Parameters")] [HideReferenceObjectPicker] [SerializeField] private Vector3Reference smallModelScale;
    [FoldoutGroup("Size Parameters")] [HideReferenceObjectPicker] [SerializeField] private Vector3Reference mediumModelScale;
    [FoldoutGroup("Size Parameters")] [HideReferenceObjectPicker] [SerializeField] private Vector3Reference largeModelScale;
    [SerializeField] private List<SizeControlledFloatReference> SizeControlledProperties;

    private Vector3 moveDirection;
    private InputAction playerMove;

    private PlayerSize currentSize;
    public PlayerSize CurrentSize { get => currentSize; set => SetPlayerSize(value); }
    public Transform AimTarget => aimTargetter.CurrentTarget;
    public KinematicCharacterMotor CharacterMotor => charMotor;
    public CharacterGroundingReport GroundingStatus => charMotor.GroundingStatus;
    public CharacterTransientGroundingReport LastGroundingStatus => charMotor.LastGroundingStatus;
    private Vector3Reference[] modelScales;
    private Vector3 capsuleProportions;
    private MeatClumpController[] meatClumps;

    private Vector3 impulseVelocity;

    public delegate void _OnStartUpdateVelocity(Vector3 currentVelocity, Vector3 impulseVelocity);
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
        this.enabled = true;
        playerMove = InputManager.Instance.GetPlayerMove_Action();
        InputManager.Instance.onJump_Pressed += () => JumpPressed.Value = true;
        InputManager.Instance.onJump_Released += () => JumpPressed.Value = false;

        InputManager.Instance.onJump_Pressed += () => JumpTrigger.Activate();
        InputManager.Instance.onAttack += () => AttackTrigger.Activate();
        InputManager.Instance.onDownwardAttack += () => DownwardAttackTrigger.Activate();
        InputManager.Instance.onGroundPound += () => GroundPoundTrigger.Activate();
        InputManager.Instance.onCallMeatClump += () => CallMeatClump.Activate();

        meatClumps = meatClumpContainer.GetComponentsInChildren<MeatClumpController>(true);
        foreach(MeatClumpController meatClump in meatClumps) {
            meatClump.SetPlayerController(this);
        }

        modelScales = new Vector3Reference[] {smallModelScale, mediumModelScale, largeModelScale};
        capsuleProportions = new Vector3(charMotor.Capsule.radius, charMotor.Capsule.height, charMotor.Capsule.center.y);

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
        if (onStartUpdateVelocity != null) onStartUpdateVelocity.Invoke(currentVelocity, impulseVelocity);
        
        // if (impulseVelocity.RoundNearZero() != Vector3.zero) Debug.Log($"Current: {currentVelocity}, Add: {impulseVelocity}, New {NewVelocity.Value}");
        
        currentVelocity = NewVelocity.Value;
        impulseVelocity = Vector3.zero;

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
        if (hitCollider.gameObject.layer == layerMapper.GetLayer(LayerEnum.Bounce))
        {
            UngroundMotor();
            AddImpulse(Vector3.up * 30f);
        }
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
        if (hitCollider.gameObject.layer == layerMapper.GetLayer(LayerEnum.Bounce))
        {
            UngroundMotor();
            AddImpulse(Vector3.up * 30f);
        }
    }

    #endregion
    
    #region Player Controller Interface
    public MeatClumpController DetachClump(Vector3 direction) {
        foreach (MeatClumpController meatClump in meatClumps) {
            if (meatClump.transform.parent != null) {
                meatClump.transform.parent = null;
                meatClump.gameObject.SetActive(true);
                
                if (!unlimitedClumps) CurrentSize -= 1;
                
                //Did not normalize direction so that nearly vertical forward throws move player less horizontally
                AddImpulse(-direction.xoz() * ClumpThrowKnockbackSpeed.Value);
                return meatClump;
            }
        }
        return null;
    }

    public void AbsorbClump(MeatClumpController clump, Vector3 direction) {
        clump.transform.parent = meatClumpContainer;
        clump.gameObject.SetActive(false);
        
        if (!unlimitedClumps) CurrentSize += 1;
        AddImpulse(direction * ClumpReturnKnockbackSpeed.Value);
    }

    public void RecallClump() {
        foreach(MeatClumpController meatClump in meatClumps) {
            if(meatClump.transform.parent == null && !meatClump.ReturningToPlayer) {
                meatClump.SetReturnToPlayer();
                return;
            }
        }
    }

    public void GiveThrowKnockback(Vector3 direction)
    {
        AddImpulse(direction * ClumpThrowKnockbackSpeed.Value);
    }
    
    public void AddImpulse(Vector3 addImpulse)
    {
        this.impulseVelocity = addImpulse;
        AddImpulseTrigger.Activate();
    }
    #endregion

    private void UpdateParameters()
    {
        IsGrounded.Value = charMotor.GroundingStatus.IsStableOnGround;
        IsOnSlidebleSlope.Value = StandingOnSlideableSlope();
        IsSlopeSlideValid.Value = IsSlopeSlideReady();
        BaseVelocity.Value = charMotor.BaseVelocity;

        // animator.SetFloat("HorizontalSpeed", Mathf.Sqrt(Mathf.Pow(NewVelocity.Value.x, 2) + Mathf.Pow(NewVelocity.Value.z, 2)));
        // animator.SetFloat("VerticalVelocity", NewVelocity.Value.y);
        // animator.SetBool("IsGrounded", IsGrounded.Value);
    }

    private bool StandingOnSlideableSlope()
    {
        float slopeAngle = Vector3.Angle(GroundingStatus.GroundNormal, Vector3.up);

        if (!GroundingStatus.IsStableOnGround &&
            GroundingStatus.FoundAnyGround &&
            slopeAngle > charMotor.MaxStableSlopeAngle &&
            slopeAngle < MaxSlopeSlideAngle.Value)
            return true;

        return false;
    }

    private bool IsSlopeSlideReady()
    {
        //Stop timer when land on stable ground if not already stopped
        if (GroundingStatus.IsStableOnGround && !SlopeSlideTimer.IsStopped)
            SlopeSlideTimer.StopTimer();
        
        //Start timer when land on Slope if not already started
        if (StandingOnSlideableSlope() && SlopeSlideTimer.IsStopped)
            SlopeSlideTimer.StartTimer();
        
        SlopeSlideTimer.UpdateTime();

        if (Mathf.Approximately(SlopeSlideTimer.RemainingTime, 0f))
            return true;
        else
            return false;
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
        if(value < PlayerSize.Small || value > PlayerSize.Large) return;
        else currentSize = value;

        int intSize = (int)currentSize;
        PlayerCurrentSize.Value = intSize;

        LeanTween.scale(sizeChangePivot.Value.gameObject, modelScales[intSize].Value, changeSizeTime.Value / 1000);
        Vector3 capsuleSize = Vector3.Scale(modelScales[intSize].Value, capsuleProportions);
        charMotor.SetCapsuleDimensions(capsuleSize.x, capsuleSize.y, capsuleSize.z);
        
        foreach(SizeControlledFloatReference prop in SizeControlledProperties) {
            prop.UpdateValue(currentSize);
        }
    }

    private void OnTriggerStay(Collider other) {
        GameObject otherGameObject = other.gameObject;
        if(otherGameObject.layer == layerMapper.GetLayer(LayerEnum.EnemyJumpTrigger))
        {
            AttemptJumpAttack(other, otherGameObject);
        }
    }

    private void AttemptJumpAttack(Collider enemyCollider, GameObject enemyObject)
    {
        EnemyJumpHurtTrigger enemyController = enemyObject.GetComponent<EnemyJumpHurtTrigger>();
        if (enemyController == null) return;
            
        float playerBottomY = Collider.bounds.center.y - Collider.bounds.extents.y;
        float enemyTriggerBottomY = enemyCollider.bounds.center.y - enemyCollider.bounds.extents.y;
             
        //Only jump attack if player is above bottom of enemy trigger and falling downwards
        if(playerBottomY > enemyTriggerBottomY && NewVelocity.Value.y <= 0f) {
            enemyController.DamageEnemy(1);
            JumpAttackTrigger.Activate();
            CameraShakeManager.Instance.ShakeCamera(1.75f, .3f, .3f);
            EffectsManager.Instance?.PlayClipAtPoint(jumpAttackClip, transform.position, .4f);
        }
    }
    
    private void OnGUI() {
        if (!DebugManager.Instance.EnableDebugGUI) return;

        if (GUI.Button(new Rect(5, 300, 210, 20),
            "Freeze Size: " + ((freezeSize) ? "<Enabled>" : "<Disabled>"))) freezeSize = !freezeSize;
        if (GUI.Button(new Rect(5, 323, 210, 20),
            "Unlimited Clumps: " + ((unlimitedClumps) ? "<Enabled>" : "<Disabled>"))) unlimitedClumps = !unlimitedClumps;
    }
}
