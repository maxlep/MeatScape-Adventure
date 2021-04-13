using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using Pathfinding;
using UnityEngine;

[TaskCategory("AStarPro")]
[TaskDescription("Moves AStar Seeker towards a target and completes when within stopping distance.")]
public class AStarSeek : Action
{
    public SharedGameObject aiScriptObj;
    public SharedTransform Seeker;
    public SharedTransform Target;
    public SharedFloat StoppingDistance;
    
    [BehaviorDesigner.Runtime.Tasks.Tooltip("Only compare X and Z when checking if reached destination")]
    public SharedBool IgnoreYValue;
    
    private AIDestinationSetter destinationSetter;

    public override void OnAwake()
    {
        if (aiScriptObj.Value != null)
            destinationSetter = aiScriptObj.Value.GetComponentInChildren<AIDestinationSetter>();
        else
            destinationSetter = GetComponent<AIDestinationSetter>();
    }

    public override TaskStatus OnUpdate()
    {
        //Set target desination
        destinationSetter.target = Target.Value;

        //Complete when stopping distance away
        float sqrDistance;
        if (IgnoreYValue.Value)
            sqrDistance = (Seeker.Value.position.xoz() - Target.Value.position.xoz()).sqrMagnitude;
        else
            sqrDistance = (Seeker.Value.position - Target.Value.position).sqrMagnitude;
        
        float distToCheckSqr = Mathf.Pow(StoppingDistance.Value, 2f);

        if (sqrDistance > distToCheckSqr)
            return TaskStatus.Running;
        
        return TaskStatus.Success;
    }
    
    public override void OnReset()
    {
        Target.Value = null;
        StoppingDistance.Value = 0;
        destinationSetter = null;
    }
}
