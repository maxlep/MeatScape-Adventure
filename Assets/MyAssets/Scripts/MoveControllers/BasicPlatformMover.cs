using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class BasicPlatformMover : MonoBehaviour, IMoverController
{
    [SerializeField] private PhysicsMover Mover;

    [SerializeField] private Vector3 TranslationAxis = Vector3.right;
    [SerializeField] private float TranslationPeriod = 10;
    [SerializeField] private float TranslationSpeed = 1;
    [SerializeField] [Range(0, 1)] private float TranslationTimeOffset = 0;
    [SerializeField] private Vector3 RotationAxis = Vector3.up;
    [SerializeField] private float RotSpeed = 10;
    [SerializeField] private Vector3 OscillationAxis = Vector3.zero;
    [SerializeField] private float OscillationPeriod = 10;
    [SerializeField] private float OscillationSpeed = 10;
    [SerializeField] [Range(0, 1)] private float RotationTimeOffset = 0;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
        
    private void Start()
    {
        _originalPosition = Mover.Rigidbody.position;
        _originalRotation = Mover.Rigidbody.rotation;

        Mover.MoverController = this;
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        float positionInitialOffset = TranslationTimeOffset * TranslationPeriod;
        goalPosition = (_originalPosition + (TranslationAxis.normalized * Mathf.Sin(Time.time * TranslationSpeed + positionInitialOffset) * TranslationPeriod));

        float rotationInitialOffset = RotationTimeOffset * OscillationPeriod;
        Quaternion targetRotForOscillation = Quaternion.Euler(OscillationAxis.normalized * (Mathf.Sin(Time.time * OscillationSpeed) * OscillationPeriod)) * _originalRotation;
        goalRotation = Quaternion.Euler(RotationAxis * RotSpeed * (Time.time + rotationInitialOffset)) * targetRotForOscillation;
    }
}
