using System;
using System.Collections;
using System.Collections.Generic;
using AmplifyShaderEditor;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [SerializeField] private GameObject pauseText;

    private bool isPaused = false;
    private float beforeTimeScale;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        InputManager.Instance.onPauseGame += TogglePauseGame;
        pauseText.SetActive(false);
    }

    public void TogglePauseGame()
    {
        if (!isPaused)
        {
            beforeTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
        }
        else
        {
            Time.timeScale = beforeTimeScale;
            isPaused = false;
        }
        pauseText.SetActive(isPaused);
    }
}
