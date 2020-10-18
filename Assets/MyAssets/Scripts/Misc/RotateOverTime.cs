using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    [SerializeField] private bool UseGlobalAxis = true;
    [SerializeField] private bool UsePhysics = false;
    [SerializeField] private Vector3 RotationAxis = Vector3.up;
    [SerializeField] private float RotSpeed = 10;

    private bool enableRotation = true;
    private Rigidbody rb;

    private void Awake()
    {
        if (UsePhysics)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) Debug.LogError($"Gameobject {gameObject.name} is missing Rigidbody component!");
        }
    }

    private void Update()
    {
        if (!enableRotation || UsePhysics) return;

        //Rotate with Transform
        if (UseGlobalAxis)
        {
            transform.Rotate(RotationAxis.normalized, RotSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Rotate(RotationAxis.normalized, RotSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void FixedUpdate()
    {
        if (!enableRotation || !UsePhysics) return;

        //Rotate with Physics
        if (UseGlobalAxis)
        {
            Vector3 localAxis = transform.InverseTransformDirection(RotationAxis.normalized);
            Quaternion rot = Quaternion.AngleAxis(RotSpeed * Time.deltaTime, localAxis.normalized);
            rb.MoveRotation(rb.rotation * rot);
        }
        else
        {
            Quaternion rot = Quaternion.AngleAxis(RotSpeed * Time.deltaTime, RotationAxis.normalized);
            rb.MoveRotation(rb.rotation * rot);
        }
    }

    public void StartRotating()
    {
        enableRotation = true;
    }

    public void StopRotating()
    {
        enableRotation = false;
    }
}
