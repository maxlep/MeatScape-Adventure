using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public interface ITransitionCondition
{
    void Init(string transitionName);
    bool Evaluate(List<TriggerVariable> receivedTriggers);
}
