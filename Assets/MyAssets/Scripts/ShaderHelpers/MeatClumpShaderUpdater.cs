using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.ShaderHelpers
{
    [RequireComponent(typeof(Renderer))]
    public class MeatClumpShaderUpdater : MonoBehaviour
    {
        [SerializeField, LabelText("Meat Clump Renderer")] private Renderer renderer;
        [SerializeField] private float splatTime;
        [SerializeField] private AudioClip splatSound;

        [SerializeField] private float fadeTime;
        
        private Material material;
        private bool collided;

        private void Awake()
        {
            material = renderer.material;
        }

        public void StartSplat(RaycastHit hit)
        {
            // if (collided) return;
            // collided = true;
            
            EffectsManager.Instance.PlayClipAtPoint(splatSound, transform.position);

            material.SetVector("_SplatNormal", hit.normal);

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