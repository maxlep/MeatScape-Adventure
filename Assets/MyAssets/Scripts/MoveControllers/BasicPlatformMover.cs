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
    [SerializeField] private bool IsLocalRotation;
    [SerializeField] private Vector3 RotationAxis = Vector3.up;
    [SerializeField] private float RotSpeed = 10;
    [SerializeField] private Vector3 OscillationAxis = Vector3.zero;
    [SerializeField] private float OscillationPeriod = 10;
    [SerializeField] private float OscillationSpeed = 10;
    [SerializeField] [Range(0, 1)] private float RotationTimeOffset = 0;

    private Vector3 rotAxis;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private void Start()
    {
        originalPosition = Mover.Rigidbody.position;
        originalRotation = Mover.Rigidbody.rotation;
        rotAxis = (IsLocalRotation) ? transform.TransformDirection(RotationAxis) : RotationAxis;
        Mover.MoverController = this;
    }

    public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
    {
        float positionInitialOffset = TranslationTimeOffset * TranslationPeriod;
        goalPosition = (originalPosition + (TranslationAxis.normalized * Mathf.Sin(Time.time * TranslationSpeed + positionInitialOffset) * TranslationPeriod));

        float rotationInitialOffset = RotationTimeOffset * OscillationPeriod;
        Quaternion targetRotForOscillation = Quaternion.Euler(OscillationAxis.normalized * (Mathf.Sin(Time.time * OscillationSpeed) * OscillationPeriod)) * originalRotation;
        goalRotation = Quaternion.Euler(rotAxis * RotSpeed * (Time.time + rotationInitialOffset)) * targetRotForOscillation;
    }
}
