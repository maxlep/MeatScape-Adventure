using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField] private bool useSceneReference;
    [SerializeField] private bool ignoreY;
    [SerializeField] private bool invertLookZ;
    [SerializeField] [HideIf("$useSceneReference")] private Transform target;
    [SerializeField] [ShowIf("$useSceneReference")] private TransformSceneReference targetRef;
    
    private void Update()
    {
        Transform targetTransform = useSceneReference ? targetRef.Value : target;
        Vector3 targetDir;

        if (!ignoreY)
        {
            targetDir = (targetTransform.position - transform.position).normalized;
            if (invertLookZ) targetDir = -targetDir;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
        }
        else
        {
            targetDir = (targetTransform.position - transform.position).xoz().normalized;
            if (invertLookZ) targetDir = -targetDir;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
        }
            
    }
}
