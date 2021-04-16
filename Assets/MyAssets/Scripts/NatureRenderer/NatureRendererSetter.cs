using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualDesignCafe.Rendering.Nature;

public class NatureRendererSetter : MonoBehaviour
{
    public NatureRenderer NatureRenderer;
    public float detailDistance = 150f;


    [Button(ButtonSizes.Large)]
    public void ApplySettings()
    {
        NatureRenderer.DetailDistance = detailDistance;
    }
}
