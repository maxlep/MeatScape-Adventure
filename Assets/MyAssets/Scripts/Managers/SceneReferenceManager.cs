using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.Scripts.PoseAnimator.Components;
using Sirenix.OdinInspector;
using UnityEngine;

public class SceneReferenceManager : MonoBehaviour
{
    [SerializeField] [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
    private List<TransformScenePair> TransformSceneList = new List<TransformScenePair>();
    
    [SerializeField] [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
    private List<GameObjectScenePair> GameObjectSceneList = new List<GameObjectScenePair>();
    
    [SerializeField] [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
    private List<ParticleSystemScenePair> ParticleSystemSceneList = new List<ParticleSystemScenePair>();
    
    [SerializeField] [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
    private List<AnimatorScenePair> AnimatorSceneList = new List<AnimatorScenePair>();
    
    [SerializeField] [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
    private List<AnimatableScenePair> AnimatableSceneList = new List<AnimatableScenePair>();
    
    [SerializeField] [PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
    private List<CameraScenePair> CameraSceneList = new List<CameraScenePair>();

    private void Awake()
    {
        foreach (var transformPair in TransformSceneList)
        {
            if (transformPair.AreValuesAssigned)
                transformPair.AssignValueToReference();
        }
        foreach (var gameObjectScenePair in GameObjectSceneList)
        {
            if (gameObjectScenePair.AreValuesAssigned)
                gameObjectScenePair.AssignValueToReference();
        }
        foreach (var particleSystemScenePair in ParticleSystemSceneList)
        {
            if (particleSystemScenePair.AreValuesAssigned)
                particleSystemScenePair.AssignValueToReference();
        }
        foreach (var animatorScenePair in AnimatorSceneList)
        {
            if (animatorScenePair.AreValuesAssigned)
                animatorScenePair.AssignValueToReference();
        }
        foreach (var animatableScenePair in AnimatableSceneList)
        {
            if (animatableScenePair.AreValuesAssigned)
                animatableScenePair.AssignValueToReference();
        }
        foreach (var cameraScenePair in CameraSceneList)
        {
            if (cameraScenePair.AreValuesAssigned)
                cameraScenePair.AssignValueToReference();
        }
    }
}

[Serializable]
public class TransformScenePair
{
    [SerializeField] [LabelText("Reference")] [LabelWidth(125f)]
    [Required]
    private TransformSceneReference transformSceneReference;

    [SerializeField] [LabelText("Scene Value")] [LabelWidth(125f)]
    [Required] [SceneObjectsOnly]
    private Transform transformSceneValue;

    public bool AreValuesAssigned =>
        (transformSceneValue != null && transformSceneReference != null);
    
    public void AssignValueToReference()
    {
        transformSceneReference.Value = transformSceneValue;
    }
}

[Serializable]
public class GameObjectScenePair
{
    [SerializeField] [LabelText("Reference")] [LabelWidth(125f)]
    [Required]
    private TransformSceneReference gameObjectSceneReference;

    [SerializeField] [LabelText("Scene Value")] [LabelWidth(125f)]
    [Required] [SceneObjectsOnly]
    private Transform gameObjectSceneValue;

    public bool AreValuesAssigned =>
        (gameObjectSceneValue != null && gameObjectSceneReference != null);
    
    public void AssignValueToReference()
    {
        gameObjectSceneReference.Value = gameObjectSceneValue;
    }
}

[Serializable]
public class ParticleSystemScenePair
{
    [SerializeField] [LabelText("Reference")] [LabelWidth(125f)]
    [Required]
    private TransformSceneReference particleSystemSceneReference;

    [SerializeField] [LabelText("Scene Value")] [LabelWidth(125f)]
    [Required] [SceneObjectsOnly]
    private Transform particleSystemSceneValue;

    public bool AreValuesAssigned =>
        (particleSystemSceneValue != null && particleSystemSceneReference != null);
    
    public void AssignValueToReference()
    {
        particleSystemSceneReference.Value = particleSystemSceneValue;
    }
}

[Serializable]
public class AnimatorScenePair
{
    [SerializeField] [LabelText("Reference")] [LabelWidth(125f)]
    [Required]
    private AnimatorSceneReference animatorSceneReference;

    [SerializeField] [LabelText("Scene Value")] [LabelWidth(125f)]
    [Required] [SceneObjectsOnly]
    private Animator animatorSceneValue;

    public bool AreValuesAssigned =>
        (animatorSceneValue != null && animatorSceneReference != null);
    
    public void AssignValueToReference()
    {
        animatorSceneReference.Value = animatorSceneValue;
    }
}

[Serializable]
public class AnimatableScenePair
{
    [SerializeField] [LabelText("Reference")] [LabelWidth(125f)]
    [Required]
    private AnimatableSceneReference animatableSceneReference;

    [SerializeField] [LabelText("Scene Value")] [LabelWidth(125f)]
    [Required] [SceneObjectsOnly]
    private Animatable animatableSceneValue;

    public bool AreValuesAssigned =>
        (animatableSceneValue != null && animatableSceneReference != null);
    
    public void AssignValueToReference()
    {
        animatableSceneReference.Value = animatableSceneValue;
    }
}

[Serializable]
public class CameraScenePair
{
    [SerializeField] [LabelText("Reference")] [LabelWidth(125f)]
    [Required]
    private CameraSceneReference cameraSceneReference;

    [SerializeField] [LabelText("Scene Value")] [LabelWidth(125f)]
    [Required] [SceneObjectsOnly]
    private Camera cameraSceneValue;

    public bool AreValuesAssigned =>
        (cameraSceneValue != null && cameraSceneReference != null);
    
    public void AssignValueToReference()
    {
        cameraSceneReference.Value = cameraSceneValue;
    }
}
