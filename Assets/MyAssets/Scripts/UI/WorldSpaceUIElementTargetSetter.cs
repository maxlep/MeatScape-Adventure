using System;
using System.Collections;
using System.Collections.Generic;
using Calcatz.WorldSpaceCanvasUI;
using Sirenix.OdinInspector;
using UnityEngine;

public class WorldSpaceUIElementTargetSetter : MonoBehaviour
{
    [SerializeField] private WorldSpaceUIElement worldSpaceUIElement;
    [SerializeField] private bool useSceneReference = true;
    [SerializeField] [ShowIf("$useSceneReference")] private TransformSceneReference targetRef;
    [SerializeField] [HideIf("$useSceneReference")] private Transform target;

    private void Awake()
    {
        Transform targetTrans = (useSceneReference) ? targetRef.Value : target;
        worldSpaceUIElement.worldSpaceTargetObject = targetTrans;
    }
}
