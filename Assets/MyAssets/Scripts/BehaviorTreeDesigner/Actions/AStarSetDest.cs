using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[TaskCategory("AStarPro")]
[TaskDescription("Sets destination to transform and returns success")]
public class AStarSetDest : Action
{
    public SharedGameObject aiScriptObj;
    public SharedTransform Target;
    
    private AIDestinationSetter destinationSetter;

    public override void OnAwake()
    {
        if (aiScriptObj.Value != null)
            destinationSetter = aiScriptObj.Value.GetComponent<AIDestinationSetter>();
        else
            destinationSetter = GetComponent<AIDestinationSetter>();
    }

    public override TaskStatus OnUpdate()
    {
        //Set target desination
        destinationSetter.target = Target.Value;
        
        return TaskStatus.Success;
    }
    
    public override void OnReset()
    {
        Target.Value = null;
        destinationSetter = null;
    }
}
