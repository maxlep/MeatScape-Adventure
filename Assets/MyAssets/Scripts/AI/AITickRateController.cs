using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

public class AITickRateController : MonoBehaviour
{
    [SerializeField] private BehaviorTree behaviorTree;
    [SerializeField] private AIPath aStarAIPath;

    public void SetBehaviourUpdateInterval(float seconds)
    {
        Debug.Log($"SetBehaviourUpdateInterval {seconds}");
        BehaviorManager.instance.UpdateIntervalSeconds = seconds;
    }

    public void SetAStarUpdateInterval(float seconds)
    {
        Debug.Log($"SetAStarUpdateInterval {seconds}");
        aStarAIPath.repathRate = seconds;
    }
}
