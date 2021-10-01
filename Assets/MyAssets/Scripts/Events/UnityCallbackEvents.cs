using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class UnityCallbackEvents : MonoBehaviour
{
    public List<UnityCallbackEvent> UnityEvents;
    
    //Cache these events so dont loop through constantly
    private List<UnityCallbackEvent> OnUpdateEvents = new List<UnityCallbackEvent>();
    private List<UnityCallbackEvent> OnFixedUpdateEvents = new List<UnityCallbackEvent>();

    [System.Serializable]
    public enum UnityCallback
    {
        OnAwake,
        OnStart,
        OnEnable,
        OnDisable,
        OnUpdate,
        OnFixedUpdate,
    }

    [System.Serializable]
    public class UnityCallbackEvent
    {
        [HideLabel] public UnityCallback UnityCallback;
        
        [SerializeField]
        private bool randomDelay;
        
        [HideIf("randomDelay")] [SerializeField]
        private float delay;
        
        [ShowIf("randomDelay")] [SerializeField]
        private float delayMin;
        
        [ShowIf("randomDelay")] [SerializeField] [PropertySpace(0f, 10f)]
        private float delayMax;
        
        [HideLabel] public UnityEvent unityEvent;

        public float Delay => GetDelay();

        public float GetDelay()
        {
            return randomDelay ? Random.Range(delayMin, delayMax) : delay;
        }
    }

    private void Awake()
    {
        foreach (var e in UnityEvents)
        {
            if (e.UnityCallback == UnityCallback.OnAwake)
                e.unityEvent.Invoke();
            else if (e.UnityCallback == UnityCallback.OnUpdate)
                OnUpdateEvents.Add(e);
            else if (e.UnityCallback == UnityCallback.OnFixedUpdate)
                OnFixedUpdateEvents.Add(e);
        }
    }

    private void Start()
    {
        InvokeCallbackEvents(UnityCallback.OnStart);
    }

    private void OnEnable()
    {
        InvokeCallbackEvents(UnityCallback.OnEnable);
    }

    private void OnDisable()
    {
        InvokeCallbackEvents(UnityCallback.OnDisable);
    }

    private void Update()
    {
        if (OnUpdateEvents.Count < 1) return;
        
        OnUpdateEvents.ForEach(e =>
        {
            if (Mathf.Approximately(0f, e.Delay))
                e.unityEvent.Invoke();
            else
                LeanTween.value(0f, 1f, e.Delay)
                    .setOnComplete(_ => e.unityEvent.Invoke());
        });
    }

    private void FixedUpdate()
    {
        if (OnFixedUpdateEvents.Count < 1) return;
        
        OnFixedUpdateEvents.ForEach(e =>
        {
            if (Mathf.Approximately(0f, e.Delay))
                e.unityEvent.Invoke();
            else
                LeanTween.value(0f, 1f, e.Delay)
                    .setOnComplete(_ => e.unityEvent.Invoke());
        });
    }
    

    //Loop through list of events and invoke the ones with given callback
    private void InvokeCallbackEvents(UnityCallback unityCallback)
    {
        foreach (var e in UnityEvents)
        {
            if (e.UnityCallback == unityCallback)
            {
                if (Mathf.Approximately(0f, e.Delay))
                    e.unityEvent.Invoke();
                else
                    LeanTween.value(0f, 1f, e.Delay)
                        .setOnComplete(_ => e.unityEvent.Invoke());
            }
        }
    }
}
