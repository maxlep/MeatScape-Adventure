using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class MeateorStrikeCombat : PlayerStateNode
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMapper LayerMapper;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMask EnemyMask;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMask InteractableMask;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private FloatReference PlayerScale;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private IntReference CurrentHungerLevel;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private IntReference HungerInstantKillThreshold;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private FloatReference AttackBaseRadius;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required]
    private FloatReference KnockbackTime;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private FloatReference KnockbackSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private DynamicGameEvent MeateorStrikeCollision;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private DynamicGameEvent PlayerCollidedWithTrigger_CollisionInfo;


    #endregion

    #region Outputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")]  [Required] 
    private Vector3Reference NewVelocity;
    
    private float currentRadius;
    private List<EnemyController> enemiesDamagedList = new List<EnemyController>();

    #endregion
    

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    public override void Enter()
    {
        base.Enter();
        MeateorStrikeCollision.Subscribe(OnPlayerCollidedWith);
        PlayerCollidedWithTrigger_CollisionInfo.Subscribe(OnPlayerCollidedWith);
    }

    private void OnPlayerCollidedWith(System.Object prevCollisionInfoObj, System.Object collisionInfoObj) {
        CollisionInfo collisionInfo = (CollisionInfo) collisionInfoObj;
        
        enemiesDamagedList.Clear();
        LayerMask hitMask = EnemyMask | InteractableMask;

        //Scale the radius with player scale
        currentRadius = AttackBaseRadius.Value * PlayerScale.Value;
        
        //Overlap sphere on contact point to deal damage
        Collider[] hitColliders = Physics.OverlapSphere(collisionInfo.contactPoint, currentRadius, hitMask, QueryTriggerInteraction.Collide);
        
        foreach (var collider in hitColliders)
        {
            GameObject otherObj = collider.gameObject;
            
            if (otherObj.IsInLayerMask(EnemyMask))
                HandleEnemyHit(collider);
            else if (otherObj.IsInLayerMask(InteractableMask))
                HandleInteractableHit(collider, collisionInfo);
        }
    }
    
    private void HandleEnemyHit(Collider enemyCollider)
    {
        EnemyController enemyController = enemyCollider.GetComponentInChildren<EnemyController>();
        
        //If enemy controller not found, try hurt proxy
        if (enemyController == null)
            enemyController = enemyCollider.GetComponent<EnemyHurtProxy>().EnemyController;
            
        //Dont hit enemies more than once, they can have multiple colliders
        if (enemyController != null && !enemiesDamagedList.Contains(enemyController))
        {
            TimeManager.Instance.FreezeFrame();
            if(CurrentHungerLevel.Value >= HungerInstantKillThreshold.Value) {
                enemyController.DamageEnemy(999);
            } else {
                enemyController.DamageEnemy(1);
            }
            enemyController.KnockbackEnemy(NewVelocity.Value.normalized, KnockbackTime.Value, KnockbackSpeed.Value);
            enemiesDamagedList.Add(enemyController);
        }
    }

    private void HandleInteractableHit(Collider interactableCollider, CollisionInfo collisionInfo)
    {
        var interactableScript = interactableCollider.GetComponent<InteractionReceiver>();
        if (interactableScript == null)
            interactableScript = interactableCollider.GetComponent<InteractionReceiverProxy>()?.InteractionReceiver;
        
        if (interactableScript != null)
        {
            MeateorStrikeIntoPayload payload = new MeateorStrikeIntoPayload()
            {
                hitDir = -collisionInfo.contactNormal,
                origin = playerController.transform.position
            };
            interactableScript.ReceiveMeateorStirkeIntoInteraction(payload);
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        MeateorStrikeCollision.Unsubscribe(OnPlayerCollidedWith);
    }
}
