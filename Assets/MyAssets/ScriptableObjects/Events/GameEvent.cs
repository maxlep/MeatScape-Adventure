using System;
using System.Collections.Generic;
using MyAssets.Scripts.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Events
{
    [SynchronizedHeader]
    [CreateAssetMenu(fileName = "GameEvent", menuName = "GameEvent", order = 0)]
    public class GameEvent : ScriptableObject
    {
        [TextArea (7, 10)] [HideInInlineEditors] public string Description;
        
        private List<GameEventListener> listeners = 
            new List<GameEventListener>();

        public void Raise()
        {
            for(int i = listeners.Count -1; i >= 0; i--)
                listeners[i].OnEventRaised();
        }

        public void RegisterListener(GameEventListener listener)
        { listeners.Add(listener); }

        public void UnregisterListener(GameEventListener listener)
        { listeners.Remove(listener); }
    }
}