using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraSceneReference", menuName = "SceneReferences/CameraSceneReference", order = 0)]
public class CameraSceneReference : SceneReference
{
    [SerializeField] private Camera cameraReference;
    
    public Camera Value
    {
        get => cameraReference;
        set => cameraReference = value;
    }
}