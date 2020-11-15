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
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    private void Update()
    {
        if (target != null && !useFixedUpdate)
        {
            if (followPosition) transform.position = target.position + offset;
            if (followRotation) transform.rotation = target.rotation;
            if (followScale) transform.localScale = target.localScale;
        }
    }

    private void FixedUpdate()
    {
        if (target != null && useFixedUpdate)
        {
            if (followPosition) transform.position = target.position;
            if (followRotation) transform.rotation = target.rotation;
            if (followScale) transform.localScale = target.localScale;
        }
    }
}
