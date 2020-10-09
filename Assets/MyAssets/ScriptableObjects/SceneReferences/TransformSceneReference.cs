using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TransformSceneReference", menuName = "SceneReferences/TransformSceneReference", order = 0)]
public class TransformSceneReference : SceneReference
{
    [SerializeField] private Transform transformReference;
    
    public Transform Value
    {
        get => transformReference;
        set => transformReference = value;
    }
}
