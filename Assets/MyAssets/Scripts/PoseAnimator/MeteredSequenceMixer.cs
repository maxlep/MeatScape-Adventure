using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

using UnityEngine.Experimental.Animations;

public class MeteredSequenceMixer : SequenceMixer
{
    [SerializeField] private Vector3Reference moveVelocity;
    [SerializeField] private float meter = 0;
    [Range(0.5f, 5.0f)] public float strideLength = 2f;

    void UpdateMeter()
    {
        meter += Time.deltaTime * moveVelocity.Value.magnitude;

        if (meter >= strideLength * 2)
        {
            meter %= strideLength;
        }

        var pct = meter / strideLength / 2;
        weight = pct;
    }

    new void Update()
    {
        UpdateMeter();
        base.Update();
    }

    private void OnDrawGizmos()
    {
        var center = transform.position;
        var extents = (center + Vector3.down, center + Vector3.up);
        // Gizmos.DrawLine();
    }
}
