using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class RollCombat : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private GameObjectTriggerVariable PlayerCollidedWith;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private LayerMapper layerMapper;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private IntVariable HungerLevel;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private IntVariable HungerThreshold;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    public override void Enter()
    {
        base.Enter();
        PlayerCollidedWith.Subscribe(this.OnPlayerCollidedWith);
    }

    private void OnPlayerCollidedWith(GameObject prev, GameObject gameObject) {
        if(gameObject.layer == layerMapper.GetLayer(LayerEnum.Enemy)) {
            EnemyController enemy = gameObject.GetComponent<EnemyController>();
            if(HungerLevel.Value >= HungerThreshold.Value) {
                enemy.DamageEnemy(999);
            } else {
                enemy.KnockbackEnemy(playerController.CharacterMotor.Velocity.normalized);
            }
        }
        
    }
    
    public override void Exit()
    {
        base.Exit();
        PlayerCollidedWith.Unsubscribe(this.OnPlayerCollidedWith);
    }
}