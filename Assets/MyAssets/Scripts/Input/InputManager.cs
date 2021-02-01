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
    public delegate void _OnAttack_Pressed();
    public delegate void _OnAttack_Released();
    public delegate void _OnRoll();
    public delegate void _OnDownwardAttack();
    public delegate void _OnSave();
    public delegate void _OnLoad();
    public delegate void _OnJump_Pressed();
    public delegate void _OnJump_Released();
    public delegate void _OnInteract();
    public delegate void _OnPauseGame();
    public delegate void _OnBackspace();
    public delegate void _OnFrameForward();
    public delegate void _OnManualUpdate();
    public delegate void _OnReduceTimeScale();
    public delegate void _OnResetTimeScale();
    public delegate void _OnIncreaseTimeScale();
    public delegate void _OnEnableDebug();
    public delegate void _OnFunction();
    public delegate void _OnRestartScene();

    public event _OnControlsChanged onControlsChanged;
    public event _OnAttack_Pressed onAttack_Pressed;
    public event _OnAttack_Released onAttack_Released;
    public event _OnRoll onRoll_Pressed;
    public event _OnRoll onRoll_Released;
    public event _OnDownwardAttack onDownwardAttack;
    public event _OnSave onSave;
    public event _OnLoad onLoad;
    public event _OnJump_Pressed onJump_Pressed;
    public event _OnJump_Released onJump_Released;
    public event _OnInteract onInteract;
    public event _OnPauseGame onPauseGame;
    public event _OnBackspace onBackspace;
    public event _OnFrameForward onFrameForward;
    public event _OnManualUpdate onManualUpdate;
    public event _OnReduceTimeScale onReduceTimeScale;
    public event _OnResetTimeScale onResetTimeScale;
    public event _OnIncreaseTimeScale onIncreaseTimeScale;
    public event _OnEnableDebug onEnableDebug;
    public event _OnFunction onFunction1;
    public event _OnFunction onFunction2;
    public event _OnFunction onFunction3;
    public event _OnFunction onFunction4;
    public event _OnFunction onFunction5;
    public event _OnFunction onFunction6;
    public event _OnFunction onFunction7;
    public event _OnFunction onFunction8;
    public event _OnFunction onFunction9;
    public event _OnFunction onFunction10;
    public event _OnFunction onFunction11;
    public event _OnFunction onFunction12;
    public event _OnRestartScene onRestartScene;

    private PlayerInput _inputs;
    private InputActionMap playerActions, uiActions;
    private InputAction playerMove, playerLook, playerJump, playerRegenerateMeat, mousePosition, playerRoll, playerAttack;

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
        playerRoll = playerActions.FindAction("Roll");
        playerAttack = playerActions.FindAction("Attack");
        mousePosition = uiActions.FindAction("MousePosition");
        playerActions.Disable();
        
        playerJump.performed += OnJump_Pressed;
        playerJump.canceled += OnJump_Released;
        
        playerRoll.performed += OnRoll_Pressed;
        playerRoll.canceled += OnRoll_Released;

        playerAttack.performed += OnAttack_Pressed;
        playerAttack.canceled += OnAttack_Released;
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
    
    public void OnRoll_Pressed(InputAction.CallbackContext ctx)
    {
        if (onRoll_Pressed != null) onRoll_Pressed();
    }
    
    public void OnRoll_Released(InputAction.CallbackContext ctx)
    {
        if (onRoll_Released != null) onRoll_Released();
    }
    
    public void OnAttack_Pressed(InputAction.CallbackContext ctx)
    {
        if (onAttack_Pressed != null) onAttack_Pressed();
    }
    
    public void OnAttack_Released(InputAction.CallbackContext ctx)
    {
        if (onAttack_Released != null) onAttack_Released();
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
    
    public void OnInteract()
    {
        if (onInteract != null) onInteract();
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
    
    public void OnFunction1()
    {
        if (onFunction1 != null) onFunction1();
    }
    
    public void OnFunction2()
    {
        if (onFunction2 != null) onFunction2();
    }
    
    public void OnFunction3()
    {
        if (onFunction3 != null) onFunction3();
    }
    
    public void OnFunction4()
    {
        if (onFunction4 != null) onFunction4();
    }
    
    public void OnFunction5()
    {
        if (onFunction5 != null) onFunction5();
    }
    
    public void OnFunction6()
    {
        if (onFunction6 != null) onFunction6();
    }
    
    public void OnFunction7()
    {
        if (onFunction7 != null) onFunction7();
    }
    
    public void OnFunction8()
    {
        if (onFunction8 != null) onFunction8();
    }
    
    public void OnFunction9()
    {
        if (onFunction9 != null) onFunction9();
    }
    
    public void OnFunction10()
    {
        if (onFunction10 != null) onFunction10();
    }
    
    public void OnFunction11()
    {
        if (onFunction11 != null) onFunction11();
    }
    
    public void OnFunction12()
    {
        if (onFunction12 != null) onFunction12();
    }
    
    public void OnRestartScene()
    {
        if (onRestartScene != null) onRestartScene();
    }

    #endregion
    

    public void EnablePlayerInputActions(bool isEnabled)
    {
        if (isEnabled) playerActions.Enable();
        else playerActions.Disable();
    }

}
