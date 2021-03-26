using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField] private bool useSceneReference;
    [SerializeField] [HideIf("$useSceneReference")] private Transform target;
    [SerializeField] [ShowIf("$useSceneReference")] private TransformSceneReference targetRef;
    
    private void Update()
    {
        if (target == null) return;

        Transform targetTransform = useSceneReference ? targetRef.Value : target;
        transform.LookAt(targetTransform);
    }
}
