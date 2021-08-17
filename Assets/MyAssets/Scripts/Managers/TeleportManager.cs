using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    [SerializeField] private TransformSceneReference playerSceneRef;

    private KinematicCharacterMotor charMotor;
    private Vector3 pos1, pos2, pos3;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        InputManager.Instance.onNumpad1 += () => SavePosition(1);
        InputManager.Instance.onNumpad2 += () => LoadPosition(1);
        
        InputManager.Instance.onNumpad4 += () => SavePosition(2);
        InputManager.Instance.onNumpad5 += () => LoadPosition(2);
        
        InputManager.Instance.onNumpad7 += () => SavePosition(3);
        InputManager.Instance.onNumpad8 += () => LoadPosition(3);
    }

    private void Start()
    {
        if (playerSceneRef.Value != null)
        {
            charMotor = playerSceneRef.Value.GetComponent<KinematicCharacterMotor>();

            //Save pos 1 as the spawn position (some delay to wait for load)
            LeanTween.value(0f, 1f, 2f).setOnComplete(() => SavePosition(1));
        }
    }

    private void SavePosition(int index)
    {
        switch (index)
        {
            case (1):
                pos1 = charMotor.transform.position;
                break;
            case (2):
                pos2 = charMotor.transform.position;
                break;
            case (3):
                pos3 = charMotor.transform.position;
                break;
        }
        
        Debug.Log($"Saved Position {index}");
    }

    private void LoadPosition(int index)
    {
        switch (index)
        {
            case (1):
                charMotor.SetPosition(pos1);
                break;
            case (2):
                charMotor.SetPosition(pos2);
                break;
            case (3):
                charMotor.SetPosition(pos3);
                break;
        }
        
        Debug.Log($"Loaded Position {index}");
    }
    
}
