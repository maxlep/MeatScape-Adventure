using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CounterEvent : MonoBehaviour
{
    [SerializeField] private int Count = 1;

    public UnityEvent OnCounterComplete;
    
    private int remainingCount;
    private bool activated = false;

    private void Start()
    {
        remainingCount = Count;
    }

    public void DecrementCount(int decrement)
    {
        if (activated) return;
        
        remainingCount--;

        if (remainingCount <= 0)
        {
            OnCounterComplete.Invoke();
            activated = true;
        }
    }
}
