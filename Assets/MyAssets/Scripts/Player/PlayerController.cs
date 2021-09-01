using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Cinemachine;
using KinematicCharacterController;
using MoreMountains.Feedbacks;
using MoreMountains.NiceVibrations;
using MoreMountains.Tools;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
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

public struct GroundingInfo
{
    public bool foundGround;
    public float distance;
    public Vector3 normal;
}

public struct CollisionInfo
{
    public Collider other;
    public Vector3 contactPoint;
    public Vector3 contactNormal;
}

public class PlayerController : SerializedMonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor charMotor;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private LayerMapper layerMapper;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private AudioClip jumpAttackClip;
    [SerializeField] private AimTargeter aimTargeter;
    [SerializeField] private Collider collider;
    [SerializeField] private GameObject ragdollPrefab;
    [SerializeField] private TransformSceneReference freeLookCamRef;
    public bool invincible;
    [SerializeField] private bool noHungerDecay;
    [SerializeField] private bool noRegenFromMeatContact;
    [SerializeField] private bool ignoreInput;
    [ShowIf("ignoreInput"), SerializeField] private Vector2Reference fakeMoveInput;

    [Button]
    private void FakeJump()
    {
        JumpTrigger.Activate();
    }

    #region ReferenceInputs

    [FoldoutGroup("Referenced Inputs")] [SerializeField] private Vector3Reference NewVelocity;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private QuaternionReference NewRotation;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference StoredJumpVelocity;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference MinSlopeSlideAngle;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference MaxStableDenivelationAngle;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference Scale;
    [FoldoutGroup("Referenced Inputs")] [SerializeField] private FloatReference MeaatClumpBoostForce;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector2Reference MoveInput;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector3Reference BaseVelocity;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference JumpPressed;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference AttackPressed;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference SlingshotPressed;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference DashPressed;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference ModifierPressed;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector3Reference PreviousVelocityAfterUpdate;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private Vector3Reference PreviousVelocityDuringUpdate;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private FloatReference DistanceToGround;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private FloatReference DistanceToMeatGround;
    [FoldoutGroup("Referenced Outputs")] [SerializeField] private BoolReference IsPlayerDead;

    #endregion

    #region TransitionParameters

    [FoldoutGroup("Transition Parameters")] [SerializeField] private BoolReference IsGrounded;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private BoolReference IsOnSlidebleSlope;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private BoolReference SlingshotReady;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable AttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable AttackReleaseTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable JumpTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable JumpAttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable DownwardAttackTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable SlingshotTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable SlingshotReleaseTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable DashPressTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable AddImpulseTrigger;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable BecameGrounded;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TriggerVariable BecameUngrounded;
    [FoldoutGroup("Transition Parameters")] [SerializeField] private TimerVariable GroundSlamCooldownTimer;

    #endregion

    #region InteractionParameters

    [FoldoutGroup("Interaction Parameters")] [SerializeField] private FloatReference ClumpThrowKnockbackSpeed;
    [FoldoutGroup("Interaction Parameters")] [SerializeField] private FloatReference InvincibilityTime;

    #endregion


    [Title("Hunger value")]
    [FoldoutGroup("Hunger Parameters"), SerializeField] private LayerMask HungerSurfaceMask;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private FloatReference DistanceToMeatGroundThreshold;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private FloatReference DistancePerClump;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private TimerReference HungerDecayTimer;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private IntReference HungerSoftMax;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private IntReference HungerOut;
    [Title("Model scale")]
    [FoldoutGroup("Hunger Parameters"), SerializeField] private TransformSceneReference SizeChangePivot;
    [Title("Blend shapes")]
    [FoldoutGroup("Hunger Parameters"), SerializeField] private SkinnedMeshRenderer SkinnedMesh;
    [FoldoutGroup("Hunger Parameters"), SerializeField] private SkinnedMeshRenderer EyesSkinnedMesh;

    [Title("Frenzy value")]
    [FoldoutGroup("Frenzy Parameters"), SerializeField] private TimerReference FrenzyDecayTimer;
    [FoldoutGroup("Frenzy Parameters"), SerializeField] private IntReference FrenzyOut;

    [FoldoutGroup("Feedbacks")] [SerializeField] private MMFeedbacks sizeUpFeedback;
    [FoldoutGroup("Feedbacks")] [SerializeField] private MMFeedbacks sizeDownFeedback;

    [FoldoutGroup("Referenced Components")] [SerializeField] private Line slingshotLine;
    [FoldoutGroup("Referenced Components")] [SerializeField] private GameObject slingshotCone;

    [FoldoutGroup("GameEvents")] [SerializeField] private GameEvent throwClumpEvent;
    [FoldoutGroup("GameEvents")] [SerializeField] private DynamicGameEvent PlayerCollidedWith_CollisionInfo;
    [FoldoutGroup("GameEvents")] [SerializeField] private DynamicGameEvent PlayerCollidedWithTrigger_CollisionInfo;

    private Vector3 moveDirection;
    private InputAction playerMove;

    private GroundingInfo groundInfo, meatGroundInfo;
    private bool isMeatGrounded => DistanceToMeatGround.Value <= DistanceToMeatGroundThreshold.Value;
    private bool isRegeneratingMeat;
    private float meatDistance;

    public KinematicCharacterMotor CharacterMotor => charMotor;
    public CharacterGroundingReport GroundingStatus => charMotor.GroundingStatus;
    public CharacterTransientGroundingReport LastGroundingStatus => charMotor.LastGroundingStatus;

    public GroundingInfo GroundInfo => groundInfo;

    private List<InteractionReceiver> interactablesInRange = new List<InteractionReceiver>();

    private Vector3 impulse;
    private Vector3 impulseRedirectable;
    private Vector3 impulseOverlayed;
    private Vector3 impulseOverlayedOverrideX;
    private Vector3 overrideVelocity;

    private float lastDamageTime = -Mathf.Infinity;

    private float capsuleStartHeight, capsuleStartRadius;
    private Vector3 capsuleStartCenter;

    public float Gravity { get => gravity; set => gravity = value; }
    public float UpwardsGravityFactor { get => upwardsGravityFactor; set => upwardsGravityFactor = value; }
    public float DownwardsGravityFactor { get => downwardsGravityFactor; set => downwardsGravityFactor = value; }

    private float gravity;
    private float upwardsGravityFactor;
    private float downwardsGravityFactor;

    private CinemachineFreeLook freeLookCam;

    private bool isInvincible;
    private bool ungroundTrigger;

    public delegate void _OnStartUpdateVelocity(VelocityInfo velocityInfo);
    public delegate void _OnStartUpdateRotation(Quaternion currentRotation);
    public event _OnStartUpdateVelocity onStartUpdateVelocity;
    public event _OnStartUpdateRotation onStartUpdateRotation;

    #region Unity Lifecycle Methods

    private void Start()
    {
        charMotor.CharacterController = this;
        MaxStableDenivelationAngle.Subscribe(OnUpdateMaxStableDenivelationAngle);
        HungerDecayTimer?.RestartTimer();

        FrenzyOut.Subscribe(OnUpdateFrenzy);
        GroundSlamCooldownTimer.StartTimer();

        capsuleStartHeight = charMotor.Capsule.height;
        capsuleStartCenter = charMotor.Capsule.center;
        capsuleStartRadius = charMotor.Capsule.radius;

        UpdateScale(Scale.Value, Scale.Value);
        Scale.Subscribe(UpdateScale);
        IsPlayerDead.Value = false;
    }

    void Awake()
    {
        this.enabled = true;
#if UNITY_EDITOR
        if(ignoreInput) return;
#endif
        playerMove = InputManager.Instance.GetPlayerMove_Action();
        InputManager.Instance.onJump_Pressed += () => JumpPressed.Value = true;
        InputManager.Instance.onJump_Released += () => JumpPressed.Value = false;
        InputManager.Instance.onSlingshot_Pressed += () => SlingshotPressed.Value = true;
        InputManager.Instance.onSlingshot_Released += () => SlingshotPressed.Value = false;
        InputManager.Instance.onDash_Pressed += () => DashPressed.Value = true;
        InputManager.Instance.onDash_Released += () => DashPressed.Value = false;
        InputManager.Instance.onModifier_Pressed += () => ModifierPressed.Value = true;
        InputManager.Instance.onModifier_Released += () => ModifierPressed.Value = false;
        InputManager.Instance.onAttack_Pressed += () => AttackPressed.Value = true;
        InputManager.Instance.onAttack_Released += () => AttackPressed.Value = false;

        InputManager.Instance.onJump_Pressed += () => JumpTrigger.Activate();
        InputManager.Instance.onAttack_Pressed += () => AttackTrigger.Activate();
        InputManager.Instance.onAttack_Released += () => AttackReleaseTrigger.Activate();
        InputManager.Instance.onDownwardAttack += () => DownwardAttackTrigger.Activate();
        InputManager.Instance.onSlingshot_Pressed += () => SlingshotTrigger.Activate();
        InputManager.Instance.onSlingshot_Released += () => SlingshotReleaseTrigger.Activate();
        InputManager.Instance.onDash_Pressed += () => DashPressTrigger.Activate();
        InputManager.Instance.onInteract += AttemptInspect;
        InputManager.Instance.onFunction3 += RemoveHunger;
        InputManager.Instance.onFunction4 += FeedHunger;

        freeLookCam = freeLookCamRef.Value.GetComponent<CinemachineFreeLook>();
    }

    // Update is called once per frame.
    private void Update()
    {
        UpdateParameters();
        UpdateHunger();
        UpdateFrenzy();
    }

    private void OnDestroy()
    {
        MaxStableDenivelationAngle.Unsubscribe(OnUpdateMaxStableDenivelationAngle);
        FrenzyOut.Unsubscribe(OnUpdateFrenzy);
        Scale.Unsubscribe(UpdateScale);
    }

    void OnApplicationQuit()
    {
        //Stop all vibrations when quitting game/editor, to prevent getting stuck
        MMVibrationManager.StopContinuousHaptic(true);
        MMVibrationManager.StopAllHaptics(true);
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
        if(onStartUpdateRotation != null) onStartUpdateRotation.Invoke(currentRotation);
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
            impulseVelocity = impulse,
            impulseVelocityRedirectble = impulseRedirectable
        };

        //Only use NewVelocity if there was subscriber that handled the update
        //Otherwise, will risk using stale value from last subscriber update!
        if(onStartUpdateVelocity != null)
        {
            onStartUpdateVelocity.Invoke(velocityInfo);
            currentVelocity = NewVelocity.Value;
        }

        #region OverlayedVelocity

        if(ungroundTrigger && (impulseOverlayedOverrideX.y > 0f) ||
                                impulseOverlayed.y > 0f ||
                                StoredJumpVelocity.Value > 0f ||
                                overrideVelocity.y > 0f)
        {
            UngroundMotor();
        }

        //This stored velocity is additive
        if(!Mathf.Approximately(0f, impulseOverlayed.sqrMagnitude))
        {
            currentVelocity += impulseOverlayed;
            NewVelocity.Value = currentVelocity; //Update new velocity to keep in sync
        }

        //This stored velocity overrides current x velocity
        if(!Mathf.Approximately(0f, impulseOverlayedOverrideX.sqrMagnitude))
        {
            currentVelocity = impulseOverlayedOverrideX;
            NewVelocity.Value = currentVelocity; //Update new velocity to keep in sync
        }

        //This stored velocity overrides current y velocity
        if(!Mathf.Approximately(StoredJumpVelocity.Value, 0f))
        {
            currentVelocity.y = StoredJumpVelocity.Value;
            NewVelocity.Value = currentVelocity; //Update new velocity to keep in sync
        }

        if(!Mathf.Approximately(0f, overrideVelocity.magnitude))
        {
            currentVelocity = overrideVelocity;
            NewVelocity.Value = currentVelocity;
        }

        #endregion

        #region Reset Vars

        impulse = Vector3.zero;
        impulseRedirectable = Vector3.zero;
        impulseOverlayed = Vector3.zero;
        impulseOverlayedOverrideX = Vector3.zero;
        overrideVelocity = Vector3.zero;
        ungroundTrigger = false;

        #endregion

        PreviousVelocityDuringUpdate.Value = currentVelocity;
    }

    public void AfterCharacterUpdate(float deltaTime, Vector3 previousVelocity)
    {
        // This is called after the motor has finished everything in its update
        StoredJumpVelocity.Value = 0f;
        PreviousVelocityAfterUpdate.Value = previousVelocity;


        #region Rigidbody Sweep Test

        var sweepHits = playerRb.SweepTestAll(previousVelocity.normalized,
            previousVelocity.magnitude * deltaTime, QueryTriggerInteraction.Collide);

        foreach(var hit in sweepHits)
        {
            GameObject other = hit.collider.gameObject;

            //Activate Force Effectors
            if(other.layer == layerMapper.GetLayer(LayerEnum.Bounce) ||
                other.layer == layerMapper.GetLayer(LayerEnum.Interactable) ||
                other.layer == layerMapper.GetLayer(LayerEnum.InteractableTarget))
            {
                ForceEffector forceEffector = other.GetComponent<ForceEffector>();
                if(forceEffector != null)
                    forceEffector.Activate(this);
            }

            //Broadcast trigger collisions info
            if(hit.collider.isTrigger)
            {
                CollisionInfo collisionInfo = new CollisionInfo()
                {
                    other = hit.collider,
                    contactPoint = hit.point,
                    contactNormal = hit.normal
                };
                PlayerCollidedWithTrigger_CollisionInfo.Raise(collisionInfo);
            }
        }
        #endregion
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
        CollisionInfo collisionInfo = new CollisionInfo()
        {
            other = hitCollider,
            contactPoint = hitPoint,
            contactNormal = hitNormal
        };
        PlayerCollidedWith_CollisionInfo.Raise(collisionInfo);
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

    #region Player Controller Interface

    public void OnClumpThrown(Vector3 direction, bool canRedirect = false)
    {
        AddImpulse(-direction.xoz() * ClumpThrowKnockbackSpeed.Value, canRedirect);
        throwClumpEvent.Raise();
    }

    public void GiveThrowKnockback(Vector3 direction)
    {
        AddImpulse(direction * ClumpThrowKnockbackSpeed.Value);
    }

    //Add an impulse that is managed and applied by the current state
    public void AddImpulse(Vector3 addImpulse, bool canRedirect = false)
    {
        if(canRedirect) impulseRedirectable += addImpulse;
        else impulse += addImpulse;
        AddImpulseTrigger.Activate();
    }

    //Add an impulse that is independent of state and is applied at player controller level
    public void AddImpulseOverlayed(Vector3 addImpulse, bool overrideXComponent = false, bool ungroundMotor = false)
    {
        if(overrideXComponent)
            impulseOverlayedOverrideX += addImpulse;
        else
            impulseOverlayed += addImpulse;

        ungroundTrigger = ungroundMotor;
        AddImpulseTrigger.Activate();
    }

    //Directly set the velocity of player
    public void SetVelocity(Vector3 velocity, bool ungroundMotor = false)
    {
        ungroundTrigger = ungroundMotor;
        overrideVelocity = velocity;
    }

    //Set an overlayed impulse that is needed to move player a certain distance in a direction
    //Takes into account current gravity and factors
    public void SetImpulseDistance(Vector3 dir, float distance, bool additiveY = false)
    {
        float gravityFactor = gravity;

        if(dir.y > 0)
            gravityFactor *= upwardsGravityFactor;
        else
            gravityFactor *= downwardsGravityFactor;

        Vector3 impulse = Mathf.Sqrt(-2f * gravityFactor * distance) * dir;

        if(additiveY)
            AddImpulseOverlayed(impulse.oyo());
        else
            StoredJumpVelocity.Value = impulse.y;


        AddImpulseOverlayed(impulse.xoz());
    }

    public void Damage(int damage, Vector3 knockbackDir, float knockbackSpeed)
    {
        //State invincibility
        if(isInvincible || invincible) return;

        //Invincibility time after damage
        if(lastDamageTime + InvincibilityTime.Value > Time.time)
            return;

        this.AddImpulse(knockbackDir * knockbackSpeed);

        if(HungerOut.Value <= 0)
            OnDeath();
        else
            IncrementHunger(-damage);

        lastDamageTime = Time.time;
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

        if(arrowVector.sqrMagnitude > 0f)
            slingshotCone.transform.rotation = Quaternion.LookRotation(arrowVector);
    }

    public void UngroundMotor(float time = .1f)
    {
        charMotor.ForceUnground(time);

        //If on ground and calling unground, activate became ungrounded trigger
        if(GroundingStatus.FoundAnyGround || LastGroundingStatus.FoundAnyGround)
            BecameUngrounded.Activate();
    }

    public void SetPlayerPosition(Vector3 position)
    {
        charMotor.SetPosition(position);
    }

    public void SetInvincible(bool enabled)
    {
        isInvincible = enabled;
    }

    public Vector3 GetPlatformVelocity()
    {
        return charMotor.Velocity - charMotor.BaseVelocity;
    }

    public void ApplyMeatClumpBoost()
    {
        Vector3 force = PreviousVelocityAfterUpdate.Value.normalized * MeaatClumpBoostForce.Value;
        AddImpulseOverlayed(force);
    }

    #endregion

    #region Hunger

    private void UpdateHunger()
    {
        HungerDecayTimer?.UpdateTime();
        if(HungerDecayTimer.IsFinished && HungerOut.Value > 0 && !noHungerDecay)
        {
            IncrementHunger(-1);
            HungerDecayTimer.RestartTimer();
        }

        if(isMeatGrounded && isRegeneratingMeat && !noRegenFromMeatContact)
        {
            meatDistance += PreviousVelocityAfterUpdate.Value.magnitude * Time.deltaTime;
            if(meatDistance >= DistancePerClump.Value)
            {
                meatDistance %= DistancePerClump.Value;
                IncrementHunger(1);
            }
        }
        else if(!isMeatGrounded)
        {
            meatDistance = 0;
        }
    }

    public void UpdateScale(float prevValue, float value)
    {
        //SizeChangePivot.Value.localScale = new Vector3(value, value, value);
        if(prevValue > value) sizeDownFeedback?.PlayFeedbacks();
        else sizeUpFeedback?.PlayFeedbacks();

        charMotor.SetCapsuleDimensions(
            capsuleStartRadius * Scale.Value,
            capsuleStartHeight * Scale.Value,
            capsuleStartCenter.y - capsuleStartHeight / 2 + Scale.Value * capsuleStartHeight * 0.5f
        );
    }

    public void SetRegenerating(bool value)
    {
        isRegeneratingMeat = value;
    }

    public void ResetMeatDistance()
    {
        meatDistance = 0;
    }

    public void UpdateStarvation(float value)
    {
        var clamped = Mathf.Clamp(value, 0, 100);
        SkinnedMesh.SetBlendShapeWeight(0, clamped);
    }

    public void UpdateBloat(float value)
    {
        var clamped = Mathf.Clamp(value, 0, 100);
        SkinnedMesh.SetBlendShapeWeight(1, clamped);
        //EyesSkinnedMesh.SetBlendShapeWeight(0, clamped);
    }

    [Button]
    private void FeedHunger()
    {
        IncrementHunger(1);
    }

    [Button]
    private void RemoveHunger()
    {
        IncrementHunger(-1);
    }

    public void IncrementHunger(int amount)
    {
        HungerOut.Value += amount;
        HungerOut.Value = Mathf.Max(HungerOut.Value, 0);
    }

    #endregion

    #region Frenzy

    private void UpdateFrenzy()
    {
        FrenzyDecayTimer?.UpdateTime();
        if(FrenzyDecayTimer.IsFinished)
        {
            FrenzyOut.Value = 0;
            FrenzyDecayTimer.StopTimer();
        }
    }

    [Button]
    private void AddFrenzy()
    {
        FrenzyDecayTimer.RestartTimer();
        FrenzyOut.Value += 1;
    }

    private void OnUpdateFrenzy()
    {
        if(FrenzyOut.Value > 0)
        {
            FrenzyDecayTimer.RestartTimer();
        }
    }

    #endregion

    #region Death

    private void OnDeath()
    {
        gameObject.SetActive(false);
        IsPlayerDead.Value = true;
        GameObject ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        Rigidbody[] ragdollsRbs = ragdoll.GetComponentsInChildren<Rigidbody>();
        ragdollsRbs.ForEach(r => r.velocity = PreviousVelocityAfterUpdate.Value);
        freeLookCam.Follow = ragdollsRbs[0].transform;
        freeLookCam.LookAt = ragdollsRbs[0].transform;

        //Force stop all vibrations to prevent getting stuck
        LeanTween.value(0f, 1f, .5f)
            .setOnComplete(_ => {
                MMVibrationManager.StopContinuousHaptic(true);
                MMVibrationManager.StopAllHaptics(true);
            });

    }

    #endregion

    #region Input / Update Parameters

    private void GetInput()
    {
#if UNITY_EDITOR
        if(ignoreInput)
        {
            MoveInput.Value = fakeMoveInput.Value;
            return;
        }
#endif
        MoveInput.Value = playerMove.ReadValue<Vector2>();
    }

    private void OnUpdateMaxStableDenivelationAngle()
    {
        charMotor.MaxStableDenivelationAngle = MaxStableDenivelationAngle.Value;
    }

    private void UpdateParameters()
    {
        IsGrounded.Value = charMotor.GroundingStatus.FoundAnyGround;
        if(charMotor.GroundingStatus.FoundAnyGround) SlingshotReady.Value = true;
        IsOnSlidebleSlope.Value = StandingOnSlideableSlope();
        BaseVelocity.Value = charMotor.BaseVelocity;
        groundInfo = GetGroundInfo(charMotor.StableGroundLayers);
        DistanceToGround.Value = groundInfo.distance;
        meatGroundInfo = GetGroundInfo(HungerSurfaceMask);
        DistanceToMeatGround.Value = meatGroundInfo.distance;
        GroundSlamCooldownTimer.UpdateTime();

        if(!LastGroundingStatus.FoundAnyGround && GroundingStatus.FoundAnyGround)
            BecameGrounded.Activate();



        else if(LastGroundingStatus.FoundAnyGround && !GroundingStatus.FoundAnyGround)
            BecameUngrounded.Activate();
    }

    private bool StandingOnSlideableSlope()
    {
        if(!GroundingStatus.FoundAnyGround)
            return false;

        float slopeAngle = Vector3.Angle(GroundingStatus.GroundNormal, Vector3.up);

        if(slopeAngle > MinSlopeSlideAngle.Value)
            return true;

        return false;
    }

    //Cast downward from collider to get info about ground below
    private GroundingInfo GetGroundInfo(LayerMask groundMask)
    {
        GroundingInfo info = new GroundingInfo();
        RaycastHit hit;
        var offset = Vector3.up * collider.bounds.size.y;
        bool foundGroundBelow = CastColliderDown(out hit, offset, Mathf.Infinity, groundMask);
        if(foundGroundBelow)
        {
            info.foundGround = true;
            info.normal = hit.normal;
            info.distance = hit.distance - offset.y;
        }
        else
        {
            info.foundGround = false;
            info.normal = Vector3.zero;
            info.distance = Mathf.Infinity;
        }

        return info;
    }

    private bool CastColliderDown(out RaycastHit hit, Vector3 offset, float dist, LayerMask groundMask)
    {
        var bounds = collider.bounds;
        var center = bounds.center + (Vector3.up * (-bounds.extents.y + bounds.extents.z));
        if(Physics.SphereCast(center + offset, collider.bounds.extents.z, Vector3.down, out hit,
            dist, groundMask))
        {
            return true;
        }
        return false;
    }

    #endregion

    #region TriggerCallbacks

    private void OnTriggerStay(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        if(otherGameObject.layer == layerMapper.GetLayer(LayerEnum.EnemyJumpTrigger) ||
            otherGameObject.IsInLayerMask(interactableMask))
        {
            AttemptJumpAttack(other);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        if(otherGameObject.IsInLayerMask(interactableMask))
        {
            InteractionReceiver interactableScript = GetInteractionReceiver(otherGameObject);
            if(interactableScript != null)
            {
                interactablesInRange.Add(interactableScript);
                interactableScript.ReceiveTriggerEnterInteraction(new TriggerEnterPayload());
            }
        }

        //Broadcast trigger collisions info
        if(other.isTrigger)
        {
            CollisionInfo collisionInfo = new CollisionInfo()
            {
                other = other,
                contactPoint = collider.bounds.center,
                contactNormal = (collider.bounds.center - other.bounds.center).normalized
            };
            PlayerCollidedWithTrigger_CollisionInfo.Raise(collisionInfo);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        if(otherGameObject.IsInLayerMask(interactableMask))
        {
            InteractionReceiver interactableScript = GetInteractionReceiver(otherGameObject);
            if(interactableScript != null)
            {
                interactablesInRange.Remove(interactableScript);
                interactableScript.ReceiveTriggerExitInteraction(new TriggerExitPayload());
            }
        }
    }

    #endregion

    #region Actions

    private void AttemptJumpAttack(Collider otherCollider)
    {
        float playerBottomY = collider.bounds.center.y - collider.bounds.extents.y;
        float otherColliderBottomY = otherCollider.bounds.center.y - otherCollider.bounds.extents.y;

        //Only jump attack if player is above bottom of trigger and falling downwards
        if(playerBottomY > otherColliderBottomY && NewVelocity.Value.y <= 0f)
        {
            InteractionReceiver interactionReceiver = GetInteractionReceiver(otherCollider.gameObject);
            if(interactionReceiver != null)
            {
                bool hasJumpInteraction = interactionReceiver.ReceiveJumpOnInteraction(new JumpOnPayload());
                if(!hasJumpInteraction) return;
            }

            JumpAttackTrigger.Activate();
            CameraShakeManager.Instance.ShakeCamera(1.75f, .3f, .3f);
            EffectsManager.Instance?.PlayClipAtPoint(jumpAttackClip, transform.position, .4f);
        }
    }

    private void AttemptInspect()
    {
        if(interactablesInRange.Count < 1) return;

        interactablesInRange[0].ReceiveInspectInteraction(new InspectPayload());
    }

    #endregion

    #region Helpers

    private InteractionReceiver GetInteractionReceiver(GameObject other)
    {
        InteractionReceiver interactionReceiver = other.GetComponent<InteractionReceiver>();
        if(interactionReceiver == null)
            interactionReceiver = other.GetComponent<InteractionReceiverProxy>()?.InteractionReceiver;

        return interactionReceiver;
    }

    #endregion




}
