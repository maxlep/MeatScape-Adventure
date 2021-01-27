using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Pathfinding;
using UnityEngine;

public class AStarSeek : Action
{
    public SharedTransform Seeker;
    public SharedTransform Target;
    public SharedFloat StoppingDistance;
    
    private AIDestinationSetter destinationSetter;

    public override void OnAwake()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
    }

    public override TaskStatus OnUpdate()
    {
        //Set target desination
        destinationSetter.target = Target.Value;

        //Complete when stopping distance away
        float sqrDistance = (Seeker.Value.position - Target.Value.position).sqrMagnitude;
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
