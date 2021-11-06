using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineFreeLook))]
public class FreeLookAddOn : MonoBehaviour
{
    [SerializeField] [TitleGroup("Gamepad")] [Range(0f, 10f)] private float LookSpeedXGamepad = .7f;
    [SerializeField] [TitleGroup("Gamepad")] [Range(0f, 10f)] private float LookSpeedYGamepad = .8f;
    [SerializeField] [TitleGroup("Gamepad")] private float AccelerationXGamepad = .04f;
    [SerializeField] [TitleGroup("Gamepad")] private float AccelerationYGamepad = .04f;
    
    [SerializeField] [TitleGroup("Mouse")] [Range(0f, 10f)] private float LookSpeedXMouse = .35f;
    [SerializeField] [TitleGroup("Mouse")] [Range(0f, 10f)] private float LookSpeedYMouse = .4f;
    [SerializeField] [TitleGroup("Mouse")] private float AccelerationXMouse = .02f;
    [SerializeField] [TitleGroup("Mouse")]  float AccelerationYMouse = .02f;
    
    [SerializeField] private bool InvertY = false;
    [SerializeField] private float recenterTimer = 3f;
    [SerializeField] private float recenterAcceleration = .1f;
    
    private CinemachineFreeLook _freeLookComponent;
    private InputAction lookInput, mousePosition;
    private float previousVelocityX = 0f;
    private float previousVelocityY = 0f;
    private float lastInputTime = Mathf.NegativeInfinity;
    

    private void Awake()
    {
        lookInput = InputManager.Instance.GetPlayerLook_Action();
        mousePosition = InputManager.Instance.GetMousePosition_Action();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    private void Start()
    {
        _freeLookComponent = GetComponent<CinemachineFreeLook>();
    }

    private void LateUpdate()
    {
        Look();
        
        if (lastInputTime + recenterTimer < Time.time)
            Recenter();
    }
    
    // Update the look movement each time the event is trigger.
    private void Look()
    {
        //If mouse is outside game view, dont rotate camera
        Vector2 mousePos = mousePosition.ReadValue<Vector2>();
        Rect screenRect = new Rect(0,0, Screen.width, Screen.height);
        if (!screenRect.Contains(mousePos))
            return;


        //Normalize the vector to have an uniform vector in whichever form it came from (I.E Gamepad, mouse, etc)
        Vector2 lookMovement = lookInput.ReadValue<Vector2>();
        if (!Mathf.Approximately(0f, lookMovement.sqrMagnitude)) lastInputTime = Time.time;
        
        lookMovement.y = InvertY ? -lookMovement.y : lookMovement.y;

        // This is because X axis is only contains between -180 and 180 instead of 0 and 1 like the Y axis
        lookMovement.x = lookMovement.x * 180f;

        float lookSpeedX = InputManager.Instance.usingMouse ? LookSpeedXMouse : LookSpeedXGamepad;
        float lookSpeedY = InputManager.Instance.usingMouse ? LookSpeedYMouse : LookSpeedYGamepad;

        float targetVelocityX = lookMovement.x * lookSpeedX;
        float targetVelocityY = lookMovement.y * lookSpeedY;

        float outVelocityX = 0f;
        float outVelocityY = 0f;
        
        float accelerationX = InputManager.Instance.usingMouse ? AccelerationXMouse : AccelerationXGamepad;
        float accelerationY = InputManager.Instance.usingMouse ? AccelerationYMouse : AccelerationYGamepad;

        //Accelerate and Deaccelerate the move velocity
        float newVelocityX = Mathf.SmoothDamp(previousVelocityX, targetVelocityX, ref outVelocityX, accelerationX);
        float newVelocityY = Mathf.SmoothDamp(previousVelocityY, targetVelocityY, ref outVelocityY, accelerationY);
        

        //Adjust axis values using look speed and Time.deltaTime so the look doesn't go faster if there is more FPS
        _freeLookComponent.m_XAxis.Value += newVelocityX * Time.deltaTime;
        _freeLookComponent.m_YAxis.Value += newVelocityY * Time.deltaTime;
        
        
    previousVelocityX = newVelocityX;
    previousVelocityY = newVelocityY;
    }

    private void Recenter()
    {
        float outVelocityY = 0f;
        
        _freeLookComponent.m_YAxis.Value =
            Mathf.SmoothDamp(_freeLookComponent.m_YAxis.Value, .5f, ref outVelocityY, recenterAcceleration);
    }
}