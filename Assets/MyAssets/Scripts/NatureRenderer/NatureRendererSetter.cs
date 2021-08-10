using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualDesignCafe.Rendering.Nature;

public class NatureRendererSetter : MonoBehaviour
{
    public NatureRenderer NatureRenderer;
    public float detailDistance = 150f;
    [Range(0f, 1f)] public float reduceDensityInDistance = .5f;
    public Vector2 ReduceDensityDistance = new Vector2(250f, 1000f);


    [Button(ButtonSizes.Large)]
    public void ApplySettings()
    {
        NatureRenderer.DetailDistance = detailDistance;
        NatureRenderer.ReduceDensityAmount = reduceDensityInDistance;
        NatureRenderer.ReduceDensityDistance = ReduceDensityDistance;
    }
}
