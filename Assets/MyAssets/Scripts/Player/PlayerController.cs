using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using KinematicCharacterController;
using MoreMountains.Feedbacks;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Player;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public struct VelocityInfo
{
    public Vector3 currentVelocity;
    public Vector3 impulseVelocity;
    public Vector3 impulseVelocityRedirectble;
}

public class PlayerController : SerializedMonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor charMotor;
    [SerializeField] private LayerMapper layerMapper;
    public CapsuleCollider Collider;
    [SerializeField] private AudioClip jumpAttackClip;
    [SerializeField] private AimTargetter aimTargetter;
    public bool invincible;

    [FoldoutGroup("Referenced Inputs")] [SerializeField] private Vector3Reference NewVelocity;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private QuaternionReference NewRotation;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference StoredJumpVelocity;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference MinSlopeSlideAngle;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference MaxStableDenivelationAngle;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private IntReference PlayerHealth;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector2Reference MoveInput;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector3Reference BaseVelocity;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference JumpPressed;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private BoolReference IsGrounded;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private BoolReference IsOnSlidebleSlope;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private IntReference PlayerCurrentSize;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable AttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable JumpTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable DownwardAttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable RollTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable RollReleaseTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable JumpAttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable AddImpulseTrigger;

    [FoldoutGroup("Interaction Parameters")] [SerializeField] private FloatReference ClumpThrowKnockbackSpeed;
    [FoldoutGroup("Interaction Parameters")] [SerializeField] private FloatReference EnemyKnockbackSpeed;
    [FoldoutGroup("Interaction Parameters")] [SerializeField] private FloatReference InvincibilityTime;

    [Title("Hunger value")]
    [FoldoutGroup("Hunger Parameters"), SerializeField] private TimerReference HungerDecayTimer;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private IntReference HungerSoftMax;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private IntReference HungerOut;
    [Title("Model scale")]
    [FoldoutGroup("Hunger Parameters"), SerializeField] private CurveReference HungerScaleCurve;
    [FoldoutGroup("Hunger Parameters")] [SerializeField] private TransformSceneReference SizeChangePivot;
    [Title("Blend shapes")]
    [FoldoutGroup("Hunger Parameters"), SerializeField] private CurveReference HungerStarveShapeCurve;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private CurveReference HungerBloatShapeCurve;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private SkinnedMeshRenderer SkinnedMesh;

    [FoldoutGroup("Feedbacks")] [SerializeField] private MMFeedbacks damageFeedback;
    
    [FoldoutGroup("Referenced Components")] [SerializeField] private Line slingshotLine;
    [FoldoutGroup("Referenced Components")] [SerializeField] private GameObject slingshotCone;
    
    [FoldoutGroup("GameEvents")] [SerializeField] private GameEvent throwClumpEvent;
    
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
    private List<Interactable> interactablesInRange = new List<Interactable>();

    private Vector3 impulseVelocity;
    private Vector3 impulseVelocityRedirectable;
    private Vector3 impulseVelocityOverlayed;

    private float invincibilityTimer = -Mathf.Infinity;

    public delegate void _OnStartUpdateVelocity(VelocityInfo velocityInfo);
    public delegate void _OnStartUpdateRotation(Quaternion currentRotation);
    public event _OnStartUpdateVelocity onStartUpdateVelocity;
    public event _OnStartUpdateRotation onStartUpdateRotation;

    #region Unity Methods

    private void Start()
    {
        charMotor.CharacterController = this;
        MaxStableDenivelationAngle.Subscribe(() =>
        {
            charMotor.MaxStableDenivelationAngle = MaxStableDenivelationAngle.Value;
        });
        HungerDecayTimer?.RestartTimer();
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
        InputManager.Instance.onRoll_Pressed += () => RollTrigger.Activate();
        InputManager.Instance.onRoll_Released += () => RollReleaseTrigger.Activate();
        InputManager.Instance.onInteract += AttemptInteract;

        modelScales = new Vector3Reference[] {smallModelScale, mediumModelScale, largeModelScale};
        capsuleProportions = new Vector3(charMotor.Capsule.radius, charMotor.Capsule.height, charMotor.Capsule.center.y);

        CurrentSize = startSize;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateParameters();
        UpdateHunger();
    }

    private void LateUpdate()
    {
        UpdateHungerAnimations();
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
        //NOTE: this was moved to states
        //Kill Y velocity if just get grounded so not to slide
        // if (charMotor.GroundingStatus.IsStableOnGround && !charMotor.LastGroundingStatus.IsStableOnGround)
        // {
        //     currentVelocity = Vector3.ProjectOnPlane(currentVelocity , charMotor.CharacterUp);
        //     currentVelocity  = charMotor.GetDirectionTangentToSurface(currentVelocity ,
        //         charMotor.GroundingStatus.GroundNormal) * currentVelocity .magnitude;
        // }
        
        VelocityInfo velocityInfo = new VelocityInfo()
        {
            currentVelocity = currentVelocity,
            impulseVelocity = impulseVelocity,
            impulseVelocityRedirectble = impulseVelocityRedirectable
        };
        
        if (onStartUpdateVelocity != null) onStartUpdateVelocity.Invoke(velocityInfo);
        
        currentVelocity = NewVelocity.Value;
        currentVelocity += impulseVelocityOverlayed;

        if (!Mathf.Approximately(StoredJumpVelocity.Value, 0f))
        {
            currentVelocity.y = StoredJumpVelocity.Value;
        }
        
        impulseVelocity = Vector3.zero;
        impulseVelocityRedirectable = Vector3.zero;
        impulseVelocityOverlayed = Vector3.zero;

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
        HandleCharacterControllerCollisions(hitCollider);
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
        HandleCharacterControllerCollisions(hitCollider);
    }

    #endregion
    
    #region Player Controller Interface
    public void OnClumpThrown(Vector3 direction, bool canRedirect = false) {
        AddImpulse(-direction.xoz() * ClumpThrowKnockbackSpeed.Value, canRedirect);
        throwClumpEvent.Raise();
    }

    public void GiveThrowKnockback(Vector3 direction)
    {
        AddImpulse(direction * ClumpThrowKnockbackSpeed.Value);
    }
    
    public void AddImpulse(Vector3 addImpulse, bool canRedirect = false)
    {
        if (canRedirect) impulseVelocityRedirectable += addImpulse;
        else impulseVelocity += addImpulse;
        AddImpulseTrigger.Activate();
    }
    
    public void AddImpulseOverlayed(Vector3 addImpulse)
    {
        impulseVelocityOverlayed += addImpulse;
        AddImpulseTrigger.Activate();
    }

    public void Damage(Vector3 knockbackDir, float knockbackSpeed)
    {
        if(Time.time > (invincibilityTimer + InvincibilityTime.Value) && !invincible) {
            PlayerHealth.Value--;
            damageFeedback.PlayFeedbacks();
            this.AddImpulse(knockbackDir * knockbackSpeed);
            invincibilityTimer = Time.time;
        }
    }

    public void ToggleArrow(bool enabled)
    {
        slingshotLine.enabled = enabled;
        slingshotCone.SetActive(enabled);
    }

    public void SetSlingshotArrow(Vector3 arrowVector)
    {
        slingshotLine.Start = Vector3.zero;
        slingshotLine.End = transform.InverseTransformDirection(arrowVector);
        slingshotCone.transform.position = slingshotLine.transform.position + arrowVector;
        slingshotCone.transform.rotation = Quaternion.LookRotation(arrowVector);
    }
    #endregion
    
    #region Hunger
    private void UpdateHunger()
    {
        HungerDecayTimer?.UpdateTime();
        var timerUp = HungerDecayTimer?.RemainingTime <= 0f;

        if (timerUp)
        {
            // TODO react when hunger reaches 0
            damageFeedback.PlayFeedbacks();
            HungerOut.Value = Mathf.Max(0, HungerOut.Value - 1);
            HungerDecayTimer.RestartTimer();
        }
    }

    private void UpdateHungerAnimations()
    {
#warning TODO test if this breaks outside 0..1, may need to clamp but would prefer not to
        var pct = (float)HungerOut.Value / HungerSoftMax.Value;
        var scale = HungerScaleCurve.Value.Evaluate(pct);
        SizeChangePivot.Value.localScale = new Vector3(scale, scale, scale);

        var starvation = HungerStarveShapeCurve.Value.Evaluate(pct);
        var bloat = HungerBloatShapeCurve.Value.Evaluate(pct);
        SkinnedMesh.SetBlendShapeWeight(0, starvation);
        SkinnedMesh.SetBlendShapeWeight(1, bloat);
    }

    [Button]
    private void FeedHunger()
    {
        HungerOut.Value += 1;
    }
    #endregion

    private void UpdateParameters()
    {
        IsGrounded.Value = charMotor.GroundingStatus.IsStableOnGround;
        IsOnSlidebleSlope.Value = StandingOnSlideableSlope();
        BaseVelocity.Value = charMotor.BaseVelocity;
    }

    public bool StandingOnSlideableSlope()
    {
        if (!GroundingStatus.FoundAnyGround)
            return false;
        
        float slopeAngle = Vector3.Angle(GroundingStatus.GroundNormal, Vector3.up);

        if (slopeAngle > MinSlopeSlideAngle.Value)
            return true;

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
    
    public Vector3 GetPlatformVelocity()
    {
        return charMotor.Velocity - charMotor.BaseVelocity;
    }
    
    private void SetPlayerSize(PlayerSize value) {
        if(value < PlayerSize.Small || value > PlayerSize.Large) return;
        else currentSize = value;

        int intSize = (int)currentSize;
        PlayerCurrentSize.Value = intSize;

        LeanTween.scale(SizeChangePivot.Value.gameObject, modelScales[intSize].Value, changeSizeTime.Value / 1000);
        Vector3 capsuleSize = Vector3.Scale(modelScales[intSize].Value, capsuleProportions);
        charMotor.SetCapsuleDimensions(capsuleSize.x, capsuleSize.y, capsuleSize.z);
        
        foreach(SizeControlledFloatReference prop in SizeControlledProperties) {
            prop.UpdateValue(currentSize);
        }
    }

    private void HandleCharacterControllerCollisions(Collider hitCollider)
    {
        if(hitCollider.gameObject.layer == layerMapper.GetLayer(LayerEnum.Enemy)) {
            Damage(hitCollider.transform.forward, EnemyKnockbackSpeed.Value);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        if (otherGameObject.layer == layerMapper.GetLayer(LayerEnum.InteractableTrigger))
        {
            Interactable interactableScript = otherGameObject.GetComponent<Interactable>();
            if (interactableScript != null) interactablesInRange.Add(interactableScript);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        if (otherGameObject.layer == layerMapper.GetLayer(LayerEnum.InteractableTrigger))
        {
            Interactable interactableScript = otherGameObject.GetComponent<Interactable>();
            if (interactableScript != null) interactablesInRange.Remove(interactableScript);
        }
    }

    private void AttemptInteract()
    {
        if (interactablesInRange.Count < 1) return;
        
        interactablesInRange[0].InvokeOnInteract();
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
