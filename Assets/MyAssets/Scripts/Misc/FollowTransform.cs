using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private LifecycleMethod updateMethod = LifecycleMethod.Update;
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;
    [SerializeField] private bool followScale;
    [SerializeField] private bool useSceneReference = true;
    [SerializeField] [ShowIf("$useSceneReference")] private TransformSceneReference targetRef;
    [SerializeField] [HideIf("$useSceneReference")] private Transform target;
    [SerializeField] private Vector3 offset;
    
    [Title("Camera events")]
    [SerializeField] private GameEvent OnPreCullEvent;
    [SerializeField] private GameEvent OnPreRenderEvent;
    [SerializeField] private GameEvent OnPostRenderEvent;

    public enum LifecycleMethod
    {
        Update,
        FixedUpdate,
        LateUpdate,
        PreCull,
        PreRender,
        PostRender
    }

    private void OnEnable()
    {
        if (OnPreCullEvent != null) OnPreCullEvent.Subscribe(PreCull);
        if (OnPreRenderEvent != null) OnPreRenderEvent.Subscribe(PreRender);
        if (OnPostRenderEvent != null) OnPostRenderEvent.Subscribe(PostRender);
    }
    
    private void OnDisable()
    {
        if (OnPreCullEvent != null) OnPreCullEvent.Unsubscribe(PreCull);
        if (OnPreRenderEvent != null) OnPreRenderEvent.Unsubscribe(PreRender);
        if (OnPostRenderEvent != null) OnPostRenderEvent.Unsubscribe(PostRender);
    }

    private void Update()
    {
        if (updateMethod != LifecycleMethod.Update) return;
        
        Transform targetTrans = (useSceneReference) ? targetRef.Value : target;
        if (targetTrans == null) return;
        
        if (followPosition) transform.position = targetTrans.position + offset;
        if (followRotation) transform.rotation = targetTrans.rotation;
        if (followScale) transform.localScale = targetTrans.localScale;
    }

    private void FixedUpdate()
    {
        if (updateMethod != LifecycleMethod.FixedUpdate) return;
        Follow();
    }

    private void LateUpdate()
    {
        if (updateMethod != LifecycleMethod.LateUpdate) return;
        Follow();
    }

    private void PreCull()
    {
        if (updateMethod != LifecycleMethod.PreCull) return;
        Follow();
    }

    private void PreRender()
    {
        if (updateMethod != LifecycleMethod.PreRender) return;
        Follow();
    }

    private void PostRender()
    {
        if (updateMethod != LifecycleMethod.PostRender) return;
        Follow();
    }

    private void Follow()
    {
        Transform targetTrans = (useSceneReference) ? targetRef.Value : target;
        if (targetTrans == null) return;
        
        if (followPosition) transform.position = targetTrans.position + offset;
        if (followRotation) transform.rotation = targetTrans.rotation;
        if (followScale) transform.localScale = targetTrans.localScale;
    }
}
