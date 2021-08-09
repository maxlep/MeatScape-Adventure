using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class BubbleMover : MonoBehaviour, IMoverController
{
    [SerializeField] private PhysicsMover Mover;
    [SerializeField] private float Speed;
    
    private void Start()
    {
        Mover.MoverController = this;
    }
    
    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        goalPosition = transform.position + Speed * deltaTime * Vector3.up;
        goalRotation = Quaternion.identity;
    }
}
