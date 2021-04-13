using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private bool useFixedUpdate;
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;
    [SerializeField] private bool followScale;
    [SerializeField] private TransformSceneReference targetRef;
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        if (targetRef.Value != null && !useFixedUpdate)
        {
            if (followPosition) transform.position = targetRef.Value.position + offset;
            if (followRotation) transform.rotation = targetRef.Value.rotation;
            if (followScale) transform.localScale = targetRef.Value.localScale;
        }
    }

    private void FixedUpdate()
    {
        if (targetRef.Value != null && useFixedUpdate)
        {
            if (followPosition) transform.position = targetRef.Value.position;
            if (followRotation) transform.rotation = targetRef.Value.rotation;
            if (followScale) transform.localScale = targetRef.Value.localScale;
        }
    }
}
