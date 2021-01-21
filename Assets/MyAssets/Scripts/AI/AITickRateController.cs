using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class AITickRateController : MonoBehaviour
{
    [SerializeField] private BehaviorTree behaviorTree;
    public float BehaviorUpdateInterval;
    private float lastBehaviorUpdateTime;

    [SerializeField] private AIPath aStarAIPath;

    private float updateId;
    
    private void Start()
    {
        updateId = Random.value;
        lastBehaviorUpdateTime = updateId;
        BehaviorManager.instance.Tick(behaviorTree);
    }

    private void Update()
    {
        if (Time.time >= lastBehaviorUpdateTime + BehaviorUpdateInterval)
        {
            ManualBehaviorUpdate();
        }
    }

    private void ManualBehaviorUpdate()
    {
        lastBehaviorUpdateTime = Time.time;
        BehaviorManager.instance.Tick(behaviorTree);
    }

    public void SetAStarUpdateInterval(float seconds)
    {
        aStarAIPath.repathRate = seconds;
    }
}
