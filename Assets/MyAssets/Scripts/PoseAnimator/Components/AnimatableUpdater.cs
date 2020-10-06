using System;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.Components
{
    public class AnimatableUpdater : MonoBehaviour
    {
        [SerializeField] private AnimatableSceneReference animatable;

        private void Update()
        {
            animatable.Value.SharedData.AnimationLayers.ForEach(i => i.Update());
        }
    }
}