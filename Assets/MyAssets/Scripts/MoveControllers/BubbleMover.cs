using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using MoreMountains.Feedbacks;
using UnityEngine;

public class BubbleMover : MonoBehaviour, IMoverController
{
    [SerializeField] private PhysicsMover Mover;
    [SerializeField] private float Speed;
    [SerializeField] private TransformSceneReference Container;

    private void Awake()
    {
        if (Container.Value != null)
            transform.parent = Container.Value;
    }

    private void Start()
    {
        Mover.SetPosition(transform.position);
        Mover.MoverController = this;
    }
    
    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        goalPosition = transform.position + Speed * deltaTime * Vector3.up;
        goalRotation = Quaternion.identity;
    }
}
