using System;
using System.Collections.Generic;
using Den.Tools;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;

public class GroundSlamAttack: PlayerStateNode
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMapper LayerMapper;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private FloatReference PlayerScale;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private FloatReference KnockbackTime;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private FloatReference KnockbackSpeed;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private FloatValueReference GroundSlamRadius;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMask EnemyMask;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private LayerMask InteractableMask;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private IntReference Damage;

    #endregion

    private List<EnemyController> enemiesDamagedList = new List<EnemyController>();

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
        Collider[] hitColliders = Physics.OverlapSphere(playerPosition, GroundSlamRadius.Value, hitMask, QueryTriggerInteraction.Collide);
        
        foreach (var collider in hitColliders)
        {
            GameObject otherObj = collider.gameObject;
            
            if (otherObj.IsInLayerMask(EnemyMask))
                HandleEnemyHit(collider, playerPosition);
            else if (otherObj.IsInLayerMask(InteractableMask))
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
        if (interactableScript == null)
            interactableScript = interactableCollider.GetComponent<InteractionReceiverProxy>()?.InteractionReceiver;
        
        if (interactableScript != null)
            interactableScript.ReceiveGroundSlamInteraction(new GroundSlamPayload()
            {
                origin = playerPosition,
                hitDir = playerController.CharacterMotor.Velocity.normalized
            });
    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();
        Color color = new Color(1f, 1f, 0f, .35f);
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, playerController.transform.position,
            GroundSlamRadius.Value, color);
    }
}
