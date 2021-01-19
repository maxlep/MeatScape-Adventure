﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "TriggerVariable", menuName = "Variables/TriggerVariable", order = 0)]
    public class TriggerVariable : ScriptableObject
    {
        public String Name => name;

        public event OnUpdate OnUpdate;
        
        public void Subscribe(OnUpdate callback)
        {
            this.OnUpdate += callback;
        }

        public void Unsubscribe(OnUpdate callback)
        {
            this.OnUpdate -= callback;
        }
        
        public void Activate()
        {
            OnUpdate?.Invoke();
        }
    }
}
