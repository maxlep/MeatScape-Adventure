using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

[TaskCategory("AStarPro")]
[TaskDescription("Stops the seeker by setting target to itself")]
public class SmoothPathStop : Action
{
    public SharedGameObject aiScriptObj;
    
    private SmoothAIPath smoothAIPath;
    
    public override void OnAwake()
    {
        if (aiScriptObj.Value != null)
            smoothAIPath = aiScriptObj.Value.GetComponent<SmoothAIPath>();
        else
            smoothAIPath = GetComponent<SmoothAIPath>();
    }

    public override void OnStart()
    {
        smoothAIPath.Stop();
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
