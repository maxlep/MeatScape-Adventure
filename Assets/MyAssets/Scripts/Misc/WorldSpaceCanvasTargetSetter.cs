using System;
using System.Collections;
using System.Collections.Generic;
using Calcatz.WorldSpaceCanvasUI;
using UnityEngine;

public class WorldSpaceCanvasTargetSetter : MonoBehaviour
{
    [SerializeField] private WorldSpaceCanvasUI _worldSpaceCanvasUi;
    [SerializeField] private TransformSceneReference camRef;

    private void Start()
    {
        _worldSpaceCanvasUi.cam = camRef.Value.GetComponent<Camera>();
    }
}
