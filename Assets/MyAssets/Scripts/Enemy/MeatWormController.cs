using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class MeatWormController : EnemyController
{
    [Title("References")]
    [SerializeField] private LayerMapper LayerMapper;
    [SerializeField] private CharacterController CharController;
    [SerializeField] private List<Transform> SegmentList;
    
    [Title("Feedbacks")]
    [SerializeField] private MMFeedbacks BurrowingFeedbacks;

    [SerializeField] private MMFeedbacks UnBurrowFeedbacks;

    [Title("Combat")]
    [SerializeField] private int Damage = 1;

    [Title("Movement")]
    [SerializeField] private float MinSegmentDistance;
    [SerializeField] private float turningDeltaDegrees = 3f;
    [SerializeField] private float HorizontalDragAir = .3f;
    [SerializeField] private float XLaunchVelocity = 40f;
    [SerializeField] private float MinYLaunchVelocity = 30f;
    [SerializeField] private float MaxYLaunchVelocity = 100f;
    [SerializeField] private float MinYBurrowVelocity = 30f;
    [SerializeField] private float MaxYBurrowVelocity = 100f;
    [SerializeField] private float UpwardDigAcceleration = 9.8f;
    [SerializeField] private float TurnAroundDigAccelerationMultiplier = 3f;
    [SerializeField] private float FallMultiplier;
    [SerializeField] private float UnBurrowRaycastOffset = 30f;
    [SerializeField] private float MoveSpeed = 20f;
    
    [Title("LayerMasks")]
    [SerializeField] private LayerMask GroundMask;

    private Transform Head
    {
        get => SegmentList[0];
        set => SegmentList[0] = value;
    }

    private Vector3 currentVelocity;
    private Vector3 moveDir;
    private Vector3 targetDir;
    private bool isGrounded, isGroundedLast;
    private bool becameGrounded, becameUngrounded;
    private bool unburrowFeedbackReady;
    
    protected override void Awake()
    {
        base.Awake();
        moveDir = Head.forward;
    }

    private void Update()
    {
        UpdateGroundingStatus();
        HandleBurrowFeedback();
        HandleUnburrowFeedback();
        TurnToTarget();
        UpdateVelocity();
        Move();
    }

    private void UpdateGroundingStatus()
    {
        isGroundedLast = isGrounded;
        
        //Raycast downwards from head, looking for ground
        RaycastHit groundHit;
        
        bool foundGroundBelow = Physics.Raycast(Head.position, Vector3.down, out groundHit,
            Mathf.Infinity, GroundMask);

        if (foundGroundBelow)
            isGrounded = false;
        else
            isGrounded = true;

        becameGrounded = !isGroundedLast && isGrounded;
        becameUngrounded = isGroundedLast && !isGrounded;
    }

    private void HandleBurrowFeedback()
    {
        if (becameGrounded)
        {
            BurrowingFeedbacks.PlayFeedbacks();
            unburrowFeedbackReady = true;
            UnBurrowFeedbacks.StopFeedbacks();
        }
        if (becameUngrounded) BurrowingFeedbacks.StopFeedbacks();
    }

    private void HandleUnburrowFeedback()
    {
        //Raycast from position in from of head to the head's origin checking for ground
        //If found ground, it means the worm is about to surface
        RaycastHit groundHit;
        Vector3 origin = Head.position + Head.forward * UnBurrowRaycastOffset;
        float dist = Vector3.Distance(origin, Head.position);
        
        bool foundGroundAhead = Physics.Raycast(origin, -Head.forward, out groundHit,
            dist, GroundMask);

        if (foundGroundAhead)
        {
            UnBurrowFeedbacks.PlayFeedbacks();
            unburrowFeedbackReady = false;
        }
    }

    private void TurnToTarget()
    {
        if (!isGrounded) return;
        
        moveDir = Vector3.RotateTowards(currentVelocity.xoz(), targetDir, 
            turningDeltaDegrees * Time.deltaTime, Mathf.Infinity);
    }

    private void UpdateVelocity()
    {
        Vector3 newVelocity;
        
        #region Horizontal Movement

        Vector3 newDir = moveDir.xoz();
        
        float newSpeed;

        if (isGrounded)
            newSpeed = MoveSpeed;
        else
            newSpeed = currentVelocity.xoz().magnitude - (currentVelocity.xoz().magnitude * HorizontalDragAir);

        newVelocity = newDir * newSpeed;

        #endregion
        
        
        #region Vertical Movement

        float fallingMultiplier = (!isGrounded && currentVelocity.y < 0f) ? FallMultiplier : 1f;
        float turnAroundMultiplier = (isGrounded && currentVelocity.y < 0f) ? TurnAroundDigAccelerationMultiplier : 1f;
        
        if (isGrounded)
            newVelocity.y = currentVelocity.y + (UpwardDigAcceleration * turnAroundMultiplier) * Time.deltaTime;
        else
            newVelocity.y = currentVelocity.y - (Gravity * fallingMultiplier) * Time.deltaTime;

        #endregion
        
        
        //If just became grounded, clamp to burrow velocity
        if (becameGrounded)
        {
            newVelocity.y = Mathf.Min(-MinYBurrowVelocity, newVelocity.y);
            newVelocity.y = Mathf.Max(-MaxYBurrowVelocity, newVelocity.y);
            
        }
        
        //If just became ungrounded, Launch worm
        if (becameUngrounded)
        {
            //Set horizontal speed to the launch X speed
            Vector3 launchVelocity = currentVelocity.xoz().normalized * XLaunchVelocity;

            //Clamp Launch vertical velocity
            launchVelocity.y = Mathf.Max(MinYLaunchVelocity, newVelocity.y);
            launchVelocity.y = Mathf.Min(MaxYLaunchVelocity, newVelocity.y);

            newVelocity = launchVelocity;
        }
        
        currentVelocity = newVelocity;
    }
    
    private void Move() {
        Vector3 deltaMove = currentVelocity * Time.smoothDeltaTime;

        //Update Velocity of head and other segments follow
        //SegmentList[0].Translate(deltaMove, Space.World);
        CharController.Move(deltaMove);
        
        if (!Mathf.Approximately(0f, deltaMove.magnitude) && 
            (!Mathf.Approximately(0f, Time.timeScale)))
            Head.rotation = Quaternion.LookRotation(deltaMove);

        for (int i = 1; i < SegmentList.Count; i++)
        {

            Transform curBodyPart = SegmentList[i];
            Transform PrevBodyPart = SegmentList[i - 1];

            float dis = Vector3.Distance(PrevBodyPart.position,curBodyPart.position);

            float T = Time.deltaTime * dis / MinSegmentDistance * currentVelocity.magnitude;

            if (T > 0.5f)
                T = 0.5f;
            
            curBodyPart.position = Vector3.Slerp(curBodyPart.position, PrevBodyPart.position, T);
            //curBodyPart.rotation = Quaternion.Slerp(curBodyPart.rotation, PrevBodyPart.rotation, T);
            
            if (!Mathf.Approximately(0f, deltaMove.magnitude) && 
                (!Mathf.Approximately(0f, Time.timeScale)))
                curBodyPart.rotation = Quaternion.LookRotation(deltaMove.normalized);
        }
    }

    public void HandleCharacterCollision(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = hit.gameObject.GetComponent<PlayerController>();
            Vector3 knockbackDir = (playerController.transform.position - hit.point).normalized;
            playerController.Damage(Damage, knockbackDir, 10f);
        }
    }

    public void SetTargetDirection(Vector3 dir)
    {
        targetDir = dir.normalized;
    }
}
