using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using UnityEngine;

public class CameraEvents : MonoBehaviour
{
    [SerializeField] private GameEvent OnPreCullEvent;
    [SerializeField] private GameEvent OnPreRenderEvent;
    [SerializeField] private GameEvent OnPostRenderEvent;

    private void OnPreCull()
    {
        OnPreCullEvent.Raise();
    }

    private void OnPreRender()
    {
        OnPreRenderEvent.Raise();
    }

    private void OnPostRender()
    {
        OnPostRenderEvent.Raise();
    }

    
}
