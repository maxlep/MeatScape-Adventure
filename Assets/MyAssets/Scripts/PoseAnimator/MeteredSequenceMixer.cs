using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
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

    // private void OnDrawGizmos()
    // {
    //     var radius = strideLength / Mathf.PI;
    //     var contact = transform.position;
    //     var center = contact + (radius * transform.up);
    //     var rotation = meter / strideLength * 180;
    //     var extents = (contact, center + (radius * transform.up), center - radius * transform.forward, center + radius * transform.forward);
    //     var rotated = (
    //         RotatePointAroundPivot(extents.Item1, center, rotation), 
    //         RotatePointAroundPivot(extents.Item2, center, rotation),
    //         RotatePointAroundPivot(extents.Item3, center, rotation),
    //         RotatePointAroundPivot(extents.Item4, center, rotation)
    //     );
    //     Gizmos.DrawLine(rotated.Item1, rotated.Item2);
    //     GreatGizmos.DrawLine(rotated.Item3, rotated.Item4, LineStyle.Dashed);
    // }
    
    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle) {
        var dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.AngleAxis(angle, transform.right) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
}
