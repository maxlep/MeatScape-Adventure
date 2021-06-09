using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[TaskCategory("AStarPro")]
[TaskDescription("Sets destination to transform and returns success")]
public class SmoothPathSetDest : Action
{
    public SharedGameObject aiScriptObj;
    public SharedTransform Target;
    
    private SmoothAIPath smoothAIPath;

    public override void OnAwake()
    {
        if (aiScriptObj.Value != null)
            smoothAIPath = aiScriptObj.Value.GetComponent<SmoothAIPath>();
        else
            smoothAIPath = GetComponent<SmoothAIPath>();
    }

    public override TaskStatus OnUpdate()
    {
        //Set target desination
        smoothAIPath.SetDestination(Target.Value);
        
        return TaskStatus.Success;
    }
    
    public override void OnReset()
    {
        Target.Value = null;
        smoothAIPath = null;
    }
}
