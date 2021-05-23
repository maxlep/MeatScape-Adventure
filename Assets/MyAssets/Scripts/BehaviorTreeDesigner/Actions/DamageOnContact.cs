using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using Pathfinding;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Combat")]
[TaskDescription("Enables damaging player on contact with enemy.")]
public class DamageOnContact : Action
{
    public LayerMapper layerMapper;
    public SharedTransform AgentTransform;
    public SharedBool IsAttacking;

    private bool currentlyAttacking;

    public override void OnStart()
    {
        //Change to enemy Layer so able to collide with player
        AgentTransform.Value.gameObject.layer = layerMapper.GetLayer(LayerEnum.Enemy);
        IsAttacking.Value = true;
    }

    public override TaskStatus OnUpdate()
    {
        // if(!IsAttacking.Value) {
          // IsAttacking.Value = true;
          //  && !currentlyAttacking
          // currentlyAttacking 
        // }
        if (IsAttacking.Value)
            return TaskStatus.Running;

        return TaskStatus.Success;
    }

    // public override void OnEnd()
    // {
        //Change to enemyAgebt Layer so dont collide with player
        // AgentTransform.Value.gameObject.layer = layerMapper.GetLayer(LayerEnum.EnemyAgent);
    // }
}
