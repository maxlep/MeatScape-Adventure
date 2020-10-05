using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    [SerializeField] private bool UseGlobalAxis = true;
    [SerializeField] private Vector3 RotationAxis = Vector3.up;
    [SerializeField] private float RotSpeed = 10;

    private void Update()
    {
        if (UseGlobalAxis)
            transform.Rotate(RotationAxis.normalized, RotSpeed * Time.deltaTime, Space.World);
        else
            transform.Rotate(RotationAxis.normalized, RotSpeed * Time.deltaTime, Space.Self);
            
    }
}
