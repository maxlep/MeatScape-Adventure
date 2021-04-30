using System.Collections.Generic;
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
    private LayerMapper layerMapper;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private FloatReference PlayerScale;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private IntReference CurrentHungerLevel;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private IntReference HungerCost;
    
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
        CurrentHungerLevel.Value = Mathf.FloorToInt(Mathf.Max(0f, CurrentHungerLevel.Value - HungerCost.Value));
    }

    private void OnPlayerCollidedWith(System.Object prevCollisionInfoObj, System.Object collisionInfoObj) {
        CollisionInfo collisionInfo = (CollisionInfo) collisionInfoObj;
        
        enemiesDamagedList.Clear();
        LayerMask hitMask = LayerMask.GetMask(
            LayerMapper.GetLayerName(LayerEnum.Enemy),
            LayerMapper.GetLayerName(LayerEnum.Interactable)
        );
        
        //Scale the radius with player scale
        currentRadius = AttackBaseRadius.Value * PlayerScale.Value;
        
        //Overlap sphere on contact point to deal damage
        Collider[] hitColliders = Physics.OverlapSphere(collisionInfo.contactPoint, currentRadius, hitMask);
        
        foreach (var collider in hitColliders)
        {
            int otherLayer = collider.gameObject.layer;
            
            if (otherLayer == LayerMapper.GetLayer(LayerEnum.Enemy))
                HandleEnemyHit(collider);
            else if (otherLayer == LayerMapper.GetLayer(LayerEnum.Interactable))
                HandleInteractableHit(collider);
        }


        foreach (var collider in hitColliders)
        {
            int otherLayer = collider.gameObject.layer;
            
            if (otherLayer == layerMapper.GetLayer(LayerEnum.Enemy))
            {
                HandleEnemyHit(collider);
            }

            else if (otherLayer == layerMapper.GetLayer(LayerEnum.Interactable))
            {
                HandleInteractableHit(collider);
            }
        }
    }
    
    private void HandleEnemyHit(Collider enemyCollider)
    {
        EnemyController enemyController = enemyCollider.GetComponentInChildren<EnemyController>();
            
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

    private void HandleInteractableHit(Collider interactableCollider)
    {
        var interactableScript = interactableCollider.GetComponent<InteractionReceiver>();
        if (interactableScript != null)
            interactableScript.ReceiveMeateorStirkeIntoInteraction(new MeateorStrikeIntoPayload());
    }
    
    public override void Exit()
    {
        base.Exit();
        MeateorStrikeCollision.Unsubscribe(OnPlayerCollidedWith);
    }
}
