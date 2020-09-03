using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "TriggerVariable", menuName = "Variables/TriggerVariable", order = 0)]
    public class TriggerVariable : Variable
    {
        public String Name => name;
        
        public void Activate()
        {
            base.BroadcastUpdate();
        }
    }
}
