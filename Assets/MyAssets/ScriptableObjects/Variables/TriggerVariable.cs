using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [SynchronizedHeader]
    [CreateAssetMenu(fileName = "TriggerVariable", menuName = "Variables/TriggerVariable", order = 0)]
    public class TriggerVariable : ScriptableObject
    {
        [TextArea (7, 10)] [HideInInlineEditors] public String Description;
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
