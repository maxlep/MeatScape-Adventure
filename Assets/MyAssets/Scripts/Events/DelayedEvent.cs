using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedEvent : MonoBehaviour
{
    [SerializeField] private float Delay = 5f;

    public UnityEvent Response;

    public void Activate()
    {
        LeanTween.value(0f, 1f, Delay).setOnComplete(_ => Response.Invoke());
    }
}
