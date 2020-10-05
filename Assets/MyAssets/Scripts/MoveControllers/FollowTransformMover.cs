using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Jobs;

public class FollowTransformMover : MonoBehaviour, IMoverController
{
    [SerializeField] private PhysicsMover Mover;

    [Required] [SerializeField] private Transform targetTransform;

    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = false;

    private void Start()
    {
        Mover.MoverController = this;
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        goalPosition = followPosition ? targetTransform.position : transform.position;
        goalRotation = followRotation ? targetTransform.rotation : transform.rotation;
    }
}
