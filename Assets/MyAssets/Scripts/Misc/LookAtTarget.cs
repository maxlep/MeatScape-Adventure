using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Update()
    {
        if (target == null) return;
        
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dirToTarget);
    }
}
