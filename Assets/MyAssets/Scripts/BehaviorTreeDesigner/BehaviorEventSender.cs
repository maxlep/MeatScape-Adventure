using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class BehaviorEventSender : MonoBehaviour
{
    [SerializeField] private BehaviorTree behaviorTree;
    
    public void InvokeBehaviorEvent(String eventName)
    {
        behaviorTree.SendEvent(eventName);
    }
}
