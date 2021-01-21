using System.Collections;
using System.Collections.Generic;
using MyAssets.Scripts.PoseAnimator.Components;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimatableSceneReference", menuName = "SceneReferences/AnimatableSceneReference", order = 0)]
public class AnimatableSceneReference : SceneReference<Animatable> {}