using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private LayerMapper layerMapper;
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private UnityEvent onStopInteract;

    public void InvokeOnInteract()
    {
        if (onInteract != null) onInteract.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        if (otherGameObject.layer == layerMapper.GetLayer(LayerEnum.Player))
        {
            onStopInteract.Invoke();
        }
    }
}
