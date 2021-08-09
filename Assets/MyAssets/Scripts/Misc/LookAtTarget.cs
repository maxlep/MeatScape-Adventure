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
    [SerializeField] [HideIf("$useSceneReference")] private Transform target;
    [SerializeField] [ShowIf("$useSceneReference")] private TransformSceneReference targetRef;
    
    private void Update()
    {
        Transform targetTransform = useSceneReference ? targetRef.Value : target;
        
        if (!ignoreY)
            transform.LookAt(targetTransform);
        else
        {
            Vector3 targetDir = (targetTransform.position - transform.position).xoz().normalized;
            transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
        }
            
    }
}
