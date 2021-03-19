using System;
using System.Collections.Generic;
using Den.Tools;
using MyAssets.ScriptableObjects.Variables;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

public class GroundSlamAttack: PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    private LayerMapper LayerMapper;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    private FloatReference PlayerScale;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    private FloatReference AttackBaseRadius;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    private FloatReference KnockbackTime;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    private FloatReference KnockbackSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    private IntReference Damage;

    private float currentRadius;
    private LayerMask enemyMask;
    private List<EnemyController> enemiesDamagedList;

    public override void Awaken()
    {
        base.Awaken();
        enemyMask = LayerMask.GetMask(LayerMapper.GetLayerName( LayerEnum.Enemy));
    }

    public override void Enter()
    {
        base.Enter();

        enemiesDamagedList.Clear();
        Vector3 playerPosition = playerController.transform.position;
        LayerMask enemyMask = LayerMask.GetMask(LayerMapper.GetLayerName( LayerEnum.Enemy));
        
        //Scale the radius with player scale
        currentRadius = AttackBaseRadius.Value * PlayerScale.Value;
        Collider[] enemyColliders = Physics.OverlapSphere(playerPosition, currentRadius, enemyMask);
        
        foreach (var enemyCollider in enemyColliders)
        {
            EnemyController enemyController = enemyCollider.GetComponentInChildren<EnemyController>();
            
            //Dont hit enemies more than once, they can have multiple colliders
            if (enemyController != null && !enemiesDamagedList.Contains(enemyController))
            {
                enemyController.DamageEnemy(Damage.Value);
                Vector3 knockbackDirection = (enemyController.transform.position - playerPosition).normalized;
                enemyController.KnockbackEnemy(knockbackDirection, KnockbackTime.Value, KnockbackSpeed.Value);
                enemiesDamagedList.Add(enemyController);
            }
        }
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();
        Color color = new Color(1f, 1f, 0f, .35f);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, playerController.transform.position,
            currentRadius, color);
    }
}
