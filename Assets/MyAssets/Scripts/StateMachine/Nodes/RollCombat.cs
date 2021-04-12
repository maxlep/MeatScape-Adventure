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
        
        if(otherObj.layer == layerMapper.GetLayer(LayerEnum.Enemy)) {
            EnemyController enemy = otherObj.GetComponent<EnemyController>();
            if(HungerLevel.Value >= HungerInstantKillThreshold.Value) {
                enemy.DamageEnemy(999);
            } else {
                enemy.KnockbackEnemy(NewVelocity.Value.normalized, KnockbackTime.Value, KnockbackSpeed.Value);
            }
        }
        
        else if (otherObj.layer == layerMapper.GetLayer(LayerEnum.Interactable))
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
