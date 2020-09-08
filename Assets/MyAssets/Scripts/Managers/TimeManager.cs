﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using AmplifyShaderEditor;
using MyAssets.ScriptableObjects.Variables;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [SerializeField] private GameObject pauseText;
    [SerializeField] private GameObject manualUpdateText;

    private bool enableDebugGUI = false;
    private bool isPaused = false;
    private bool manualUpdate = false;
    private bool skipFrame = false;
    private float timescaleFactor = 1f;

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
        InputManager.Instance.onEnableDebug += ToggleDebugMode;
        pauseText.SetActive(false);
    }

    private void LateUpdate()
    {
        if (manualUpdate && !skipFrame) Time.timeScale = 0f;
        skipFrame = false;
    }

    private void ResetTimeScale()
    {
        timescaleFactor = 1f;
        if (!isPaused) Time.timeScale = timescaleFactor;
    }

    private void SkipFrame()
    {
        Time.timeScale = 1f;
        skipFrame = true;
    }

    private void ChangeTimeScale(float percent)
    {
        timescaleFactor += percent / 100f;

        timescaleFactor = Mathf.Clamp(timescaleFactor, .1f, 10f);
        if (!isPaused) Time.timeScale = timescaleFactor;
    }

    private void TogglePauseGame()
    {
        if (manualUpdate) return;
        
        if (!isPaused) PauseGame();
        else UnPauseGame();
    }

    private void PauseGame()
    {
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

    private void ToggleDebugMode()
    {
        enableDebugGUI = !enableDebugGUI;

        if (enableDebugGUI)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnGUI()
    {
        if (!enableDebugGUI) return;

        float pivotX = 10f;
        float pivotY = 10f;
        float height = 20f;
        float verticalMargin = 3f;
        float smallButtonWidth = 70f;


        GUI.TextArea(new Rect(pivotX, pivotY, 210, 20),
            $"Timescale: {timescaleFactor}");

        if (GUI.Button(new Rect(pivotX, pivotY + (height + verticalMargin), smallButtonWidth, 20),
            $"(J) -10%")) ChangeTimeScale(-10f);
        ;
        if (GUI.Button(new Rect(pivotX + smallButtonWidth, pivotY + (height + verticalMargin), smallButtonWidth, 20),
            $"(K) Reset")) ResetTimeScale();
        if (GUI.Button(new Rect(pivotX + smallButtonWidth * 2, pivotY + (height + verticalMargin), smallButtonWidth, 20),
            $"(L) +10%")) ChangeTimeScale(10f);

        if (GUI.Button(new Rect(pivotX, pivotY + (height + verticalMargin) * 2, 210, height),
            "(P) Manual Update: " + ((manualUpdate) ? "<Enabled>" : "<Disabled>"))) ToggleManualUpdate();
    }
}
