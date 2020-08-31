using System;
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
    public delegate void _OnAttack();
    public delegate void _OnGroundPound();
    public delegate void _OnDownwardAttack();
    public delegate void _OnSave();
    public delegate void _OnLoad();
    public delegate void _OnJump_Pressed();
    public delegate void _OnJump_Released();
    public delegate void _OnRestartGame();
    public delegate void _OnPauseGame();
    public delegate void _OnBackspace();

    public event _OnControlsChanged onControlsChanged;
    public event _OnAttack onAttack;
    public event _OnGroundPound onGroundPound;
    public event _OnDownwardAttack onDownwardAttack;
    public event _OnSave onSave;
    public event _OnLoad onLoad;
    public event _OnJump_Pressed onJump_Pressed;
    public event _OnJump_Released onJump_Released;
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
        
        playerJump.performed += OnJump_Pressed;
        playerJump.canceled += OnJump_Released;
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

    public void OnAttack()
    {
        if (onAttack != null) onAttack();
    }
    
    public void OnGroundPound()
    {
        if (onGroundPound != null) onGroundPound();
    }
    
    public void OnDownwardAttack()
    {
        if (onDownwardAttack != null) onDownwardAttack();
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
        if (onJump_Pressed != null) onJump_Pressed();
    }
    
    public void OnJump_Released(InputAction.CallbackContext ctx)
    {
        if (onJump_Released != null) onJump_Released();
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
