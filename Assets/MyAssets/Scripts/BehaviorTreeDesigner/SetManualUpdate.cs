using System;
using BehaviorDesigner.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyAssets.Scripts.BehaviorTreeDesigner
{
    public class SetManualUpdate : MonoBehaviour
    {
        private void Awake()
        {
            BehaviorManager.instance.UpdateInterval = UpdateIntervalType.Manual;
        }
    }
}