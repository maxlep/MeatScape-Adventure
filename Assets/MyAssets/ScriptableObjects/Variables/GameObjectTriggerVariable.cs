using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [SynchronizedHeader]
    [CreateAssetMenu(fileName = "GameObjectTriggerVariable", menuName = "Variables/GameObjectTriggerVariable", order = 0)]
    public class GameObjectTriggerVariable : ScriptableObject
    {
        [TextArea (7, 10)] [HideInInlineEditors] public String Description;
        public String Name => name;

        public event OnUpdate<GameObject> OnUpdate;

        private GameObject previous;
        
        public void Subscribe(OnUpdate<GameObject> callback)
        {
            this.OnUpdate += callback;
        }

        public void Unsubscribe(OnUpdate<GameObject> callback)
        {
            this.OnUpdate -= callback;
        }
        
        public void Activate(GameObject go)
        {
            OnUpdate?.Invoke(previous, go);
            previous = go;
        }
    }
}

