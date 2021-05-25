using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class RollCombat : PlayerStateNode
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private DynamicGameEvent PlayerCollidedWith_CollisionInfo;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private LayerMapper layerMapper;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMask EnemyMask;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMask InteractableMask;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private IntVariable HungerLevel;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private IntVariable HungerInstantKillThreshold;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference KnockbackTime;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private FloatReference KnockbackSpeed;

    #endregion

    #region Outputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Outputs")] [Required]
    private Vector3Reference NewVelocity;
    

    #endregion
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    public override void Enter()
    {
        base.Enter();
        PlayerCollidedWith_CollisionInfo.Subscribe(this.OnPlayerCollidedWith);
    }

    private void OnPlayerCollidedWith(System.Object prevCollisionInfoObj, System.Object collisionInfoObj)
    {
        CollisionInfo collisionInfo = (CollisionInfo) collisionInfoObj;
        GameObject otherObj = collisionInfo.other.gameObject;
        
        if(otherObj.IsInLayerMask(EnemyMask)) {
            EnemyController enemyScript = otherObj.GetComponentInChildren<EnemyController>();
            
            //If no enemy controller found, look for hurt proxy
            if (enemyScript == null)
                enemyScript = otherObj.GetComponent<EnemyHurtProxy>().EnemyController;
            
            if(HungerLevel.Value >= HungerInstantKillThreshold.Value) {
                enemyScript.DamageEnemy(999);
            } else {
                enemyScript.KnockbackEnemy(NewVelocity.Value.normalized, KnockbackTime.Value, KnockbackSpeed.Value);
            }
        }
        
        else if (otherObj.IsInLayerMask(InteractableMask))
        {
            InteractionReceiver interactionReceiver = collisionInfo.other.GetComponent<InteractionReceiver>();
            if (interactionReceiver != null) interactionReceiver.ReceiveRollIntoInteraction(new RollIntoPayload());
        }
        
    }
    
    public override void Exit()
    {
        base.Exit();
        PlayerCollidedWith_CollisionInfo.Unsubscribe(this.OnPlayerCollidedWith);
    }
}
