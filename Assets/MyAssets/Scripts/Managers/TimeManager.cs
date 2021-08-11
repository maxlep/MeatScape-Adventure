using System.Collections;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [SerializeField] private GameObject pauseText;
    [SerializeField] private GameObject manualUpdateText;
    [SerializeField] private GameObject statScreen;
    [SerializeField] private BoolReference IsPlayerDead;
    [SerializeField] private GameEvent onEnableStatScreen;
    [SerializeField] private GameEvent onDisableStatScreen;

    private bool isPaused;
    private bool manualUpdate;
    private bool skipFrame;
    private bool isFrozen;
    private bool keepStatsScreenOpen;
    private float timescaleFactor = 1f;
    private float fpsRefresh = .5f;
    private float fpsRefreshTimer;
    private int fps;

    private Coroutine freezeFrameRoutine;

    #region Unity Methods

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        InputManager.Instance.onPauseGame += TogglePauseGame;
        InputManager.Instance.onFrameForward += SkipFrame;
        InputManager.Instance.onManualUpdate += ToggleManualUpdate;
        InputManager.Instance.onReduceTimeScale += () => ChangeTimeScale(-10f);
        InputManager.Instance.onResetTimeScale += ResetTimeScale;
        InputManager.Instance.onIncreaseTimeScale += () => ChangeTimeScale(10f);
        InputManager.Instance.onFunction11 += () =>
        {
            keepStatsScreenOpen = false;
            DisableStatScreen();
        };
        InputManager.Instance.onFunction12 += () =>
        {
            keepStatsScreenOpen = true;
            EnableStatScreen();
        };

        onEnableStatScreen.Subscribe(EnableStatScreen);
        onDisableStatScreen.Subscribe(DisableStatScreen);
        
        pauseText.SetActive(false);
    }

    private void Update()
    {
        if (Time.unscaledTime > fpsRefreshTimer)
        {
            fps = (int) (1f / Time.deltaTime);
            fpsRefreshTimer = Time.unscaledTime + fpsRefresh;
        }
    }

    private void LateUpdate()
    {
        if (manualUpdate && !skipFrame) Time.timeScale = 0f;
        skipFrame = false;
    }

    #endregion

    #region Timescale

    private void ResetTimeScale()
    {
        timescaleFactor = 1f;
        if (!isPaused) Time.timeScale = timescaleFactor;
    }

    private void SkipFrame()
    {
        if (!manualUpdate) return;
        
        Time.timeScale = 1f;
        skipFrame = true;
    }

    private void ChangeTimeScale(float percent)
    {
        timescaleFactor += percent / 100f;

        timescaleFactor = Mathf.Clamp(timescaleFactor, .1f, 10f);
        if (!isPaused) Time.timeScale = timescaleFactor;
    }

    #endregion

    #region Pause/UnPause

    private void TogglePauseGame()
    {
        if (IsPlayerDead.Value || manualUpdate) return;
        
        if (!isPaused) PauseGame();
        else UnPauseGame();
    }

    private void PauseGame()
    {
        if (freezeFrameRoutine != null) StopCoroutine(freezeFrameRoutine);
        Time.timeScale = 0f;
        isPaused = true;
        pauseText.SetActive(true);
    }

    private void UnPauseGame()
    {
        Time.timeScale = timescaleFactor;
        isPaused = false;
        pauseText.SetActive(false);
    }

    #endregion

    #region Freeze Frame

    public void FreezeFrame(float duration = .05f)
    {
        if (isFrozen || isPaused) return;
        freezeFrameRoutine = StartCoroutine(DoFreeze(duration));
    }
    
    private IEnumerator DoFreeze(float duration = .05f)
    {
        isFrozen = true;
        var originalScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalScale;
        isFrozen = false;
    }

    #endregion

    #region Stat Screen

    private void EnableStatScreen()
    {
        Time.timeScale = 0f;
        statScreen.SetActive(true);
    }
    
    private void DisableStatScreen()
    {
        //If enabled by debug key, dont let anything else disable it
        if (keepStatsScreenOpen) return;
        
        Time.timeScale = timescaleFactor;
        statScreen.SetActive(false);
    }

    #endregion

    #region Debug
    
    //Set manual update mode (overrides pause game)
    public void ToggleManualUpdate()
    {
        manualUpdate = !manualUpdate;

        if (manualUpdate)
        {
            PauseGame();
            manualUpdateText.SetActive(true);
        }
        else
        {
            UnPauseGame();
            manualUpdateText.SetActive(false);
        }
    }

    private void OnGUI()
    {
        if (!DebugManager.Instance.EnableDebugGUI) return;

        float pivotX = 10f;
        float pivotY = 10f;
        float height = 20f;
        float verticalMargin = 3f;
        float smallButtonWidth = 70f;


        GUI.TextArea(new Rect(pivotX, pivotY, 210, 20),
            $"Timescale: {timescaleFactor} | FPS: {fps}");

        if (GUI.Button(new Rect(pivotX, pivotY + (height + verticalMargin), smallButtonWidth, 20),
            $"(J) -10%")) ChangeTimeScale(-10f);
        ;
        if (GUI.Button(new Rect(pivotX + smallButtonWidth, pivotY + (height + verticalMargin), smallButtonWidth, 20),
            $"(K) Reset")) ResetTimeScale();
        if (GUI.Button(new Rect(pivotX + smallButtonWidth * 2, pivotY + (height + verticalMargin), smallButtonWidth, 20),
            $"(L) +10%")) ChangeTimeScale(10f);

        if (GUI.Button(new Rect(pivotX, pivotY + (height + verticalMargin) * 2, 210, height),
            "(P) Manual Update: " + ((manualUpdate) ? "<Enabled>" : "<Disabled>"))) ToggleManualUpdate();
        
        if (GUI.Button(new Rect(pivotX + 210, pivotY + (height + verticalMargin) * 2, smallButtonWidth, height),
            "(N) Next")) SkipFrame();
    }

    #endregion
}
