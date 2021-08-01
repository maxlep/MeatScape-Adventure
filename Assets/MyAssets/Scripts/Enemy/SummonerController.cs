using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonerController : EnemyController
{
    [SerializeField] private Transform targetingReticle;
    
    protected override void Awake()
    {
        base.Awake();
        
        behaviorTree.SetVariableValue("TargetingReticleTransform", targetingReticle);
    }
}
