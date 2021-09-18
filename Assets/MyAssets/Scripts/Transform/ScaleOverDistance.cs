using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOverDistance : MonoBehaviour
{
    [SerializeField] private Transform scaleFromTransform;
    [SerializeField] private float minScaleFactor = 1f;
    [SerializeField] private float maxScaleFactor = 10f;
    [SerializeField] private float minSqrDistance;
    [SerializeField] private float maxSqrDistance = 1000f;

    private Vector3 startScale;

    private void Awake()
    {
        startScale = transform.localScale;
    }

    private void Update()
    {
        if (scaleFromTransform == null) return;

        float distanceFromTransform = (transform.position - scaleFromTransform.position).sqrMagnitude;
        float percentToMaxDistance = (distanceFromTransform - minSqrDistance) / (maxSqrDistance - minSqrDistance);
        float scaleFactor = Mathf.Lerp(minScaleFactor, maxScaleFactor, percentToMaxDistance);
        transform.localScale = startScale * scaleFactor;
    }
}
