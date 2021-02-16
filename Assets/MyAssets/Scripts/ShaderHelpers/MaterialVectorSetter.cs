using System;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using UnityEngine;

namespace MyAssets.Scripts.ShaderHelpers
{
    public class MaterialVectorSetter : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private Renderer renderer;
        [SerializeField] private int materialIndex;
        [SerializeField] private string propertyName;
        [SerializeField] private Vector3Reference variable;

        private Material material;
        private Vector2 offset;

        private void Awake()
        {
            material = renderer.materials[materialIndex];
            offset = Vector2.zero;
        }

        private void Update()
        {
            var delta = mainCamera.worldToCameraMatrix * variable.Value * Time.deltaTime;
            delta.y = 0;
            offset += delta.xy();

            material.SetVector(propertyName, offset);
            Debug.Log($"Set vector {material.GetVector(propertyName)}");
        }
    }
}