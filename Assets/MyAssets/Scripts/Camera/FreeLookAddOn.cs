using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineFreeLook))]
public class FreeLookAddOn : MonoBehaviour
{
    [SerializeField] [Range(0f, 10f)] private float LookSpeedX = 1f;
    [SerializeField] [Range(0f, 10f)] private float LookSpeedY = 1f;
    [SerializeField] private bool InvertY = false;
    
    private CinemachineFreeLook _freeLookComponent;
    private InputAction lookInput;

    private void Awake()
    {
        lookInput = InputManager.Instance.GetPlayerLook_Action();
    }

    private void Start()
    {
        _freeLookComponent = GetComponent<CinemachineFreeLook>();
    }

    private void LateUpdate()
    {
        Look();
    }

    // Update the look movement each time the event is trigger
    private void Look()
    {
        //Normalize the vector to have an uniform vector in whichever form it came from (I.E Gamepad, mouse, etc)
        Vector2 lookMovement = lookInput.ReadValue<Vector2>();
        lookMovement.y = InvertY ? -lookMovement.y : lookMovement.y;

        // This is because X axis is only contains between -180 and 180 instead of 0 and 1 like the Y axis
        lookMovement.x = lookMovement.x * 180f; 

        //Ajust axis values using look speed and Time.deltaTime so the look doesn't go faster if there is more FPS
        _freeLookComponent.m_XAxis.Value += lookMovement.x * LookSpeedX * Time.deltaTime;
        _freeLookComponent.m_YAxis.Value += lookMovement.y * LookSpeedY * Time.deltaTime;
    }
}