using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

public class TestSceneManager : MonoBehaviour
{
    [SerializeField] private KinematicCharacterMotor PlayerMotor;
    [SerializeField] [Required] private List<Transform> TeleportPoints = new List<Transform>();

    private void Awake()
    {
        InputManager.Instance.onRestartScene += RestartScene;
        
        InputManager.Instance.onFunction2 += () => TeleportPlayer(0);
        InputManager.Instance.onFunction3 += () => TeleportPlayer(1);
        InputManager.Instance.onFunction4 += () => TeleportPlayer(2);
        InputManager.Instance.onFunction5 += () => TeleportPlayer(3);
        InputManager.Instance.onFunction6 += () => TeleportPlayer(4);
        InputManager.Instance.onFunction7 += () => TeleportPlayer(5);
        InputManager.Instance.onFunction8 += () => TeleportPlayer(6);
        InputManager.Instance.onFunction9 += () => TeleportPlayer(7);
        InputManager.Instance.onFunction10 += () => TeleportPlayer(8);
        InputManager.Instance.onFunction11 += () => TeleportPlayer(9);
        InputManager.Instance.onFunction12 += () => TeleportPlayer(10);
    }

    private void TeleportPlayer(int index)
    {
        Debug.Log($"Teleport {index}");

        if (index < TeleportPoints.Count && TeleportPoints[index] != null)
        {
            Debug.Log($"DO Teleport {index}");
            PlayerMotor.SetPosition(TeleportPoints[index].position);
        }
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
