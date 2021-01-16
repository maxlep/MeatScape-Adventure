using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

public class AITickRateController : MonoBehaviour
{
    [SerializeField] private TransformSceneReference playerSceneTransform;
    [SerializeField] private float sqrDistUpdateRateInSeconds = 1f;
    
    [BoxGroup("Behavior Designer")] [SerializeField] private BehaviorTree behaviorTree;
    [BoxGroup("Behavior Designer")] [SerializeField] private float behaviorUpdateTime = 0f;
    [BoxGroup("Behavior Designer")] [SerializeField] private float behaviorUpdateTimeReduced = .25f;
    [BoxGroup("Behavior Designer")] [SerializeField] private float behaviorReduceUpdateDistance = 300f;
    [BoxGroup("Behavior Designer")] [SerializeField] private float behaviorCullDistance = 500f;
    
    [BoxGroup("AStar Pro")] [SerializeField] private AIPath aStarAIPath;
    [BoxGroup("AStar Pro")] [SerializeField] private float aStarUpdateTime = .4f;
    [BoxGroup("AStar Pro")] [SerializeField] private float aStarUpdateTimeReduced = 3f;
    [BoxGroup("AStar Pro")] [SerializeField] private float aStarReduceUpdateDistance = 300f;
    [BoxGroup("AStar Pro")] [SerializeField] private float aStarCullDistance = 500f;

    private float lastDistCheckTime = Mathf.NegativeInfinity;
    private float lastBehaviorUpdateTime = Mathf.NegativeInfinity;
    private float currentUpdateTimeBehavior;

    private void Start()
    {
        //Make sure all behaviors set to manual update
        BehaviorManager.instance.UpdateInterval = UpdateIntervalType.Manual;
    }

    private void Update()
    {
        BehaviorManualUpdate();
        
        //Limit how often check distance to player
        if (lastDistCheckTime + sqrDistUpdateRateInSeconds > Time.time)
            return;

        float sqrDistanceToPlayer = Vector3.SqrMagnitude(transform.position - playerSceneTransform.Value.position);
        
        UpdateBehaviorParameters(sqrDistanceToPlayer);
        UpdateAStarParameters(sqrDistanceToPlayer);
    }

    private void BehaviorManualUpdate()
    {
        if (lastBehaviorUpdateTime + currentUpdateTimeBehavior < Time.time)
        {
            BehaviorManager.instance.Tick(behaviorTree);
            lastBehaviorUpdateTime = Time.time;
        }
    }

    private void UpdateAStarParameters(float sqrDistanceToPlayer)
    {
        if (sqrDistanceToPlayer > Mathf.Pow(aStarCullDistance, 2f))
        {
            aStarAIPath.enabled = false;
        }
        else if (sqrDistanceToPlayer > Mathf.Pow(aStarReduceUpdateDistance, 2f))
        {
            aStarAIPath.enabled = true;
            aStarAIPath.repathRate = aStarUpdateTimeReduced;
        }
        else
        {
            aStarAIPath.enabled = true;
            aStarAIPath.repathRate = aStarUpdateTime;
        }
    }

    private void UpdateBehaviorParameters(float sqrDistanceToPlayer)
    {
        if (sqrDistanceToPlayer > Mathf.Pow(behaviorCullDistance, 2f))
        {
            behaviorTree.DisableBehavior();
            currentUpdateTimeBehavior = Mathf.Infinity;
        }
        else if (sqrDistanceToPlayer > Mathf.Pow(behaviorReduceUpdateDistance, 2f))
        {
            behaviorTree.EnableBehavior();
            currentUpdateTimeBehavior = behaviorUpdateTimeReduced;
        }
        else
        {
            behaviorTree.EnableBehavior();
            
            //If reduced update rate set to 0, just update every frame
            if (Mathf.Approximately(behaviorUpdateTime, 0f))
            {
                currentUpdateTimeBehavior = 0f;
            }
            else
            {
                currentUpdateTimeBehavior = behaviorUpdateTime;
            }
        }
    }
}
