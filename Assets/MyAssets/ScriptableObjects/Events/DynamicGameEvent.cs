using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Events;
using Sirenix.OdinInspector;
using UnityEngine;

[Required]
[SynchronizedHeader]
[CreateAssetMenu(fileName = "DynamicGameEvent", menuName = "DynamicGameEvent", order = 0)]
public class DynamicGameEvent : ScriptableObject
{
    [TextArea (7, 10)] [HideInInlineEditors] public String Description;
    public String Name => name;

    private List<DynamicGameEventListener> listeners = 
        new List<DynamicGameEventListener>();
    
    public event OnUpdate<System.Object> OnUpdate;

    private System.Object previous;

    #region Code-based Registration

    public void Subscribe(OnUpdate<System.Object> callback)
    {
        this.OnUpdate += callback;
    }

    public void Unsubscribe(OnUpdate<System.Object> callback)
    {
        this.OnUpdate -= callback;
    }

    #endregion


    #region DynamicGameEventListener Registration

    public void RegisterListener(DynamicGameEventListener listener)
    { listeners.Add(listener); }

    public void UnregisterListener(DynamicGameEventListener listener)
    { listeners.Remove(listener); }

    #endregion
    
        
    public void Raise(System.Object obj)
    {
        OnUpdate?.Invoke(previous, obj);
        
        //Broadcast to monobehavior listeners
        for(int i = listeners.Count -1; i >= 0; i--)
            listeners[i].OnEventRaised(obj);
        
        previous = obj;
    }
}
