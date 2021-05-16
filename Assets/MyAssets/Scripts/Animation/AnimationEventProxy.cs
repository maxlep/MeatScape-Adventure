using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventProxy : MonoBehaviour
{
    [HideLabel]
    [SerializeField]
    private List<AnimEvent> AnimEventList = new List<AnimEvent>();

    [Serializable]
    public struct AnimEvent
    {
        [PropertySpace(0f, 10f)]
        public string eventName;
        public UnityEvent unityEvent;
    }

    public void ReceiveEvent(string eventName)
    {
        foreach (var animEvent in AnimEventList)
        {
            if (animEvent.eventName.Equals(eventName))
                animEvent.unityEvent.Invoke();
        }
    }
}
