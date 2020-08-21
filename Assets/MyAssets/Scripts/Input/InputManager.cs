﻿using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    
    public bool usingMouse { get; private set; }

    public delegate void _OnControlsChanged(PlayerInput inputs);
    public delegate void _OnStab();
    public delegate void _OnSave();
    public delegate void _OnLoad();
    public delegate void _OnJump();
    public delegate void _OnSlash();
    public delegate void _OnUseBomb();
    public delegate void _OnWater_Pressed();
    public delegate void _OnWater_Released();
    public delegate void _OnRestartGame();
    public delegate void _OnPauseGame();
    public delegate void _OnBackspace();

    public event _OnControlsChanged onControlsChanged;
    public event _OnStab onStab;
    public event _OnSave onSave;
    public event _OnLoad onLoad;
    public event _OnJump onJump;
    public event _OnSlash onSlash;
    public event _OnUseBomb onUseBomb;
    public event _OnWater_Pressed onWater_Pressed;
    public event _OnWater_Released onWater_Released;
    public event _OnRestartGame onRestartGame;
    public event _OnPauseGame onPauseGame;
    public event _OnBackspace onBackspace;

    private PlayerInput _inputs;
    private InputActionMap playerActions, uiActions;
    private InputAction playerMove, playerLook, playerWater, playerJump;

    public static bool PlatformInvertsScroll()
    {
        Debug.Log($"Platform: {Application.platform}, OS: {SystemInfo.operatingSystem}");
        // bool isWebGl = Application.platform == RuntimePlatform.WebGLPlayer;
        // bool isWebGlOnUnix = isWebGl && (new[] {"mac", "linux"}).Any(str => SystemInfo.operatingSystem.ToLower().Contains(str));
        bool isUnix = Application.platform == RuntimePlatform.LinuxPlayer ||
                      Application.platform == RuntimePlatform.LinuxEditor ||
                      Application.platform == RuntimePlatform.OSXPlayer ||
                      Application.platform == RuntimePlatform.OSXEditor;
        return isUnix;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        _inputs = GetComponent<PlayerInput>();
        playerActions = _inputs.actions.FindActionMap("Player", true);
        uiActions = _inputs.actions.FindActionMap("UI", true);
        playerMove = playerActions.FindAction("Move");
        playerLook = playerActions.FindAction("Look");
        playerWater = playerActions.FindAction("Water");
        playerJump = playerActions.FindAction("Jump");
        playerActions.Disable();

        playerWater.performed += OnWater_Pressed;
        playerWater.canceled += OnWater_Released;
        playerJump.started += OnJump_Pressed;
    }

    void Start()
    {
        onControlsChanged?.Invoke(_inputs);
        usingMouse = _inputs.currentControlScheme == "Keyboard&Mouse";
    }

    private void Update()
    {
        #warning Inefficient checking for input; update this later!
        OnControlsChanged(_inputs);
    }

    void OnEnable()
    {
        uiActions.Enable();
        playerActions.Enable();
    }

    void OnDisable()
    {
        playerActions.Disable();
        uiActions.Disable();
        playerWater.performed -= OnWater_Pressed;
        playerWater.canceled -= OnWater_Released;
    }

    #region Action Getters

    public InputAction GetPlayerMove_Action()
    {
        return playerMove;
    }

    public InputAction GetPlayerLook_Action()
    {
        return playerLook;
    }

    #endregion

    #region Input callbacks

    public void OnControlsChanged(PlayerInput inputs)
    {
        if (onControlsChanged != null) onControlsChanged(inputs);
        usingMouse = inputs.currentControlScheme == "Keyboard&Mouse";
    }

    public void OnStab()
    {
        if (onStab != null) onStab();
    }
    
    public void OnSave()
    {
        if (onSave != null) onSave();
    }
    
    public void OnLoad()
    {
        if (onLoad != null) onLoad();
    }
    
    public void OnJump_Pressed(InputAction.CallbackContext ctx)
    {
        if (onJump != null) onJump();
    }

    public void OnSlash()
    {
        if (onSlash != null) onSlash();
    }

    public void OnUseBomb()
    {
        if (onUseBomb != null) onUseBomb();
    }
    
    public void OnWater_Pressed(InputAction.CallbackContext ctx)
    {
        if (onWater_Pressed != null) onWater_Pressed();
    }
    
    public void OnWater_Released(InputAction.CallbackContext ctx)
    {
        if (onWater_Released != null) onWater_Released();
    }

    public void OnRestartGame()
    {
        if (onRestartGame != null) onRestartGame();
    }

    public void OnPauseGame()
    {
        if (onPauseGame != null) onPauseGame();
    }

    public void OnBackspace()
    {
        if (onBackspace != null) onBackspace();
    }

    #endregion
    

    public void EnablePlayerInputActions(bool isEnabled)
    {
        if (isEnabled) playerActions.Enable();
        else playerActions.Disable();
    }

}