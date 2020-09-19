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
    [SerializeField] private float accelerationX = .025f;
    [SerializeField] private float accelerationY = .025f;
    [SerializeField] private bool InvertY = false;
    
    private CinemachineFreeLook _freeLookComponent;
    private InputAction lookInput, mousePosition;
    private float previousVelocityX = 0f;
    private float previousVelocityY = 0f;

    private void Awake()
    {
        lookInput = InputManager.Instance.GetPlayerLook_Action();
        mousePosition = InputManager.Instance.GetMousePosition_Action();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        //If mouse is outside game view, dont rotate camera
        Vector2 mousePos = mousePosition.ReadValue<Vector2>();
        Rect screenRect = new Rect(0,0, Screen.width, Screen.height);
        if (!screenRect.Contains(mousePos))
            return;
        
        
        //Normalize the vector to have an uniform vector in whichever form it came from (I.E Gamepad, mouse, etc)
        Vector2 lookMovement = lookInput.ReadValue<Vector2>();
        lookMovement.y = InvertY ? -lookMovement.y : lookMovement.y;

        // This is because X axis is only contains between -180 and 180 instead of 0 and 1 like the Y axis
        lookMovement.x = lookMovement.x * 180f;

        float targetVelocityX = lookMovement.x * LookSpeedX;
        float targetVelocityY = lookMovement.y * LookSpeedY;

        float outVelocityX = 0f;
        float outVelocityY = 0f;

        //Accelerate and Deaccelerate the move velocity
        float newVelocityX = Mathf.SmoothDamp(previousVelocityX, targetVelocityX, ref outVelocityX, accelerationX);
        float newVelocityY = Mathf.SmoothDamp(previousVelocityY, targetVelocityY, ref outVelocityY, accelerationY);
        

        //Adjust axis values using look speed and Time.deltaTime so the look doesn't go faster if there is more FPS
        _freeLookComponent.m_XAxis.Value += newVelocityX * Time.deltaTime;
        _freeLookComponent.m_YAxis.Value += newVelocityY * Time.deltaTime;
        
        
    previousVelocityX = newVelocityX;
    previousVelocityY = newVelocityY;
    }
}