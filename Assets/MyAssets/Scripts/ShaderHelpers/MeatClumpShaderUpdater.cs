using System;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.ShaderHelpers
{
    [RequireComponent(typeof(Renderer))]
    public class MeatClumpShaderUpdater : MonoBehaviour
    {
        [SerializeField, LabelText("Meat Clump Renderer")] private Renderer renderer;
        [SerializeField] private float splatTime;

        [SerializeField] private MMFeedbacks splatFeedback;
        
        private Material material;
        private bool collided;

        private void Awake()
        {
            material = renderer.material;
        }

        public void StartSplat(Vector3 normal)
        {
            // if (collided) return;
            // collided = true;
            
            splatFeedback.PlayFeedbacks();
            
            material.SetVector("_SplatNormal", normal);

            LTDescr tween;
            tween = LeanTween.value(0f, 1f, splatTime)
                .setEase(LeanTweenType.easeOutElastic)
                .setOnUpdate((float value) =>
                {
                    material.SetFloat("_SplatFac", value);
                });
        }

        public void ReverseSplat()
        {
            // material.SetVector("_SplatNormal", hit.normal);

            LTDescr tween;
            tween = LeanTween.value(1f, 0f, splatTime)
                .setEase(LeanTweenType.easeOutElastic)
                .setOnUpdate((float value) =>
                {
                    material.SetFloat("_SplatFac", value);
                });
        }
    }
}