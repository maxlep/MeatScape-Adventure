using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedEvent : MonoBehaviour
{
    [SerializeField] private float Delay = 5f;

    public UnityEvent Response;

    private LTDescr delayTween;
    private bool isTweening;
    private float tweenStartTime;

    public void Activate()
    {
        isTweening = true;
        tweenStartTime = Time.time;
    }

    private void Update()
    {
        if (!isTweening) return;

        if (tweenStartTime + Delay <= Time.time)
        {
            Response.Invoke();
            isTweening = false;
        }
    }
}
