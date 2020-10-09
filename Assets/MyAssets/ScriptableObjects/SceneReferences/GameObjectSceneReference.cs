﻿using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GameObjectSceneReference", menuName = "SceneReferences/GameObjectSceneReference", order = 0)]
public class GameObjectSceneReference : SceneReference
{
    [SerializeField] private GameObject gameObjectReference;
    
    public GameObject Value
    {
        get => gameObjectReference;
        set => gameObjectReference = value;
    }
}
