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
    private List<EnemyController> enemiesDamagedList;

    public override void Awaken()
    {
        base.Awaken();
    }

    public override void Enter()
    {
        base.Enter();

        enemiesDamagedList.Clear();
        Vector3 playerPosition = playerController.transform.position;
        LayerMask hitMask = LayerMask.GetMask(
            LayerMapper.GetLayerName( LayerEnum.Enemy),
            LayerMapper.GetLayerName( LayerEnum.Interactable)
        );
        
        //Scale the radius with player scale
        currentRadius = AttackBaseRadius.Value * PlayerScale.Value;
        Collider[] hitColliders = Physics.OverlapSphere(playerPosition, currentRadius, hitMask);
        
        foreach (var collider in hitColliders)
        {
            int otherLayer = collider.gameObject.layer;
            
            if (otherLayer == LayerMapper.GetLayer(LayerEnum.Enemy))
                HandleEnemyHit(collider, playerPosition);
            else if (otherLayer == LayerMapper.GetLayer(LayerEnum.Interactable))
                HandleInteractableHit(collider, playerPosition);
        }
    }

    private void HandleEnemyHit(Collider enemyCollider, Vector3 playerPosition)
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

    private void HandleInteractableHit(Collider interactableCollider, Vector3 playerPosition)
    {
        var interactableScript = interactableCollider.GetComponent<InteractionReceiver>();
        if (interactableScript != null)
            interactableScript.ReceiveGroundSlamInteraction(new GroundSlamPayload()
            {
                origin = playerPosition
            });
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();
        Color color = new Color(1f, 1f, 0f, .35f);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, playerController.transform.position,
            currentRadius, color);
    }
}
