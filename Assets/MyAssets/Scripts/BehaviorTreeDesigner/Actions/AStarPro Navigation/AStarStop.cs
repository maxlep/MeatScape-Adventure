using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[TaskCategory("AStarPro")]
[TaskDescription("Stops the seeks by setting target to itself")]
public class AStarStop : Action
{
    public SharedGameObject aiScriptObj;
    
    private AIDestinationSetter destinationSetter;
    
    public override void OnAwake()
    {
        if (aiScriptObj.Value != null)
            destinationSetter = aiScriptObj.Value.GetComponent<AIDestinationSetter>();
        else
            destinationSetter = GetComponent<AIDestinationSetter>();
    }

    public override void OnStart()
    {
        destinationSetter.target = destinationSetter.transform;
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
