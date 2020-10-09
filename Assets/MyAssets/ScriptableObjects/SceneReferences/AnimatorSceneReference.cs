using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimatorSceneReference", menuName = "SceneReferences/AnimatorSceneReference", order = 0)]
public class AnimatorSceneReference : SceneReference
{
    [SerializeField] private Animator animatorReference;
    
    public Animator Value
    {
        get => animatorReference;
        set => animatorReference = value;
    }
}