using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedEvent : MonoBehaviour
{
    [SerializeField] private float Delay = 5f;

    public UnityEvent Response;

    private LTDescr delayTween;

    public void Activate()
    {
        if (delayTween != null && delayTween.id != null)
            LeanTween.cancel(delayTween.id);
        
        delayTween = LeanTween.value(0f, 1f, Delay);
        Debug.Log($"Started {gameObject.name} with ID {delayTween.id}");

        delayTween.setOnUpdate((float a) =>
        {
            Debug.LogWarning($"Processing {Mathf.Lerp(0f, Delay, a)}s {gameObject.name}");
        });

        delayTween.setOnComplete(_ =>
        {
            Debug.Log($"Finished {gameObject.name} with ID {delayTween.id}");
            Response.Invoke();
        });
    }
}
