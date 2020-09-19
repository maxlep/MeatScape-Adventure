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
    public delegate void _OnRegenerateMeat_Started();
    public delegate void _OnRegenerateMeat_Pressed();
    public delegate void _OnPauseGame();
    public delegate void _OnBackspace();
    public delegate void _OnFrameForward();
    public delegate void _OnManualUpdate();
    public delegate void _OnReduceTimeScale();
    public delegate void _OnResetTimeScale();
    public delegate void _OnIncreaseTimeScale();
    public delegate void _OnEnableDebug();

    public event _OnControlsChanged onControlsChanged;
    public event _OnAttack onAttack;
    public event _OnGroundPound onGroundPound;
    public event _OnDownwardAttack onDownwardAttack;
    public event _OnSave onSave;
    public event _OnLoad onLoad;
    public event _OnJump_Pressed onJump_Pressed;
    public event _OnJump_Released onJump_Released;
    public event _OnRegenerateMeat_Started onRegenerateMeat_Started;
    public event _OnRegenerateMeat_Pressed onRegenerateMeat_Pressed;
    public event _OnPauseGame onPauseGame;
    public event _OnBackspace onBackspace;
    public event _OnFrameForward onFrameForward;
    public event _OnManualUpdate onManualUpdate;
    public event _OnReduceTimeScale onReduceTimeScale;
    public event _OnResetTimeScale onResetTimeScale;
    public event _OnIncreaseTimeScale onIncreaseTimeScale;
    public event _OnEnableDebug onEnableDebug;

    private PlayerInput _inputs;
    private InputActionMap playerActions, uiActions;
    private InputAction playerMove, playerLook, playerJump, playerRegenerateMeat, mousePosition;

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
        playerJump = playerActions.FindAction("Jump");
        playerRegenerateMeat = playerActions.FindAction("RegenerateMeat");
        mousePosition = uiActions.FindAction("MousePosition");
        playerActions.Disable();
        
        playerJump.performed += OnJump_Pressed;
        playerJump.canceled += OnJump_Released;

        playerRegenerateMeat.started += OnRegenerateMeat_Started;
        playerRegenerateMeat.performed += OnRegenerateMeat_Pressed;
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
    
    public InputAction GetMousePosition_Action()
    {
        return mousePosition;
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

    public void OnRegenerateMeat_Started(InputAction.CallbackContext ctx)
    {
        if (onRegenerateMeat_Started != null) onRegenerateMeat_Started();
    }

    public void OnRegenerateMeat_Pressed(InputAction.CallbackContext ctx)
    {
        if (onRegenerateMeat_Pressed != null) onRegenerateMeat_Pressed();
    }

    public void OnPauseGame()
    {
        if (onPauseGame != null) onPauseGame();
    }

    public void OnBackspace()
    {
        if (onBackspace != null) onBackspace();
    }
    
    public void OnFrameForward()
    {
        if (onFrameForward != null) onFrameForward();
    }
    
    public void OnManualUpdate()
    {
        if (onManualUpdate != null) onManualUpdate();
    }
    
    public void OnReduceTimeScale()
    {
        if (onReduceTimeScale != null) onReduceTimeScale();
    }
    
    public void OnResetTimeScale()
    {
        if (onResetTimeScale != null) onResetTimeScale();
    }
    
    public void OnIncreaseTimeScale()
    {
        if (onIncreaseTimeScale != null) onIncreaseTimeScale();
    }
    
    public void OnEnableDebug()
    {
        if (onEnableDebug != null) onEnableDebug();
    }

    #endregion
    

    public void EnablePlayerInputActions(bool isEnabled)
    {
        if (isEnabled) playerActions.Enable();
        else playerActions.Disable();
    }

}
