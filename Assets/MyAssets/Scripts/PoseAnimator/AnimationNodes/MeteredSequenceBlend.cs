using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.PoseAnimator;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class MeteredSequenceBlend : SequenceBlend
{
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private Vector3Reference moveVelocity;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] [Sirenix.OdinInspector.ReadOnly] private float meter = 0;
    [HideIf("$zoom")] [LabelWidth(120)] [Range(0.5f, 5.0f)] public float strideLength = 2f;

    public override void Execute()
    {
        UpdateMeter();
        base.Execute();
    }

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
}
