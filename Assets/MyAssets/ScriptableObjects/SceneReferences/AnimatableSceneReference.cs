using System.Collections;
using System.Collections.Generic;
using MyAssets.Scripts.PoseAnimator.Components;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "AnimatableSceneReference", menuName = "SceneReferences/AnimatableSceneReference", order = 0)]
public class AnimatableSceneReference : SceneReference
{
    [SerializeField] private Animatable animatableReference;
    
    public Animatable Value
    {
        get => animatableReference;
        set => animatableReference = value;
    }
}