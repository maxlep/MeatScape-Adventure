using System;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

namespace MyAssets.Scripts.ShaderHelpers
{
    public class ProjectorSetter : MonoBehaviour
    {
        [SerializeField] private Projector projector;
        [SerializeField] private FloatVariable variable;

        private void Awake()
        {
            UpdateProjector();
            variable.Subscribe(UpdateProjector);
        }

        public void UpdateProjector()
        {
            projector.orthographicSize = variable.Value;
            Debug.Log($"Set orthographic size {variable.Value} {projector.orthographicSize}");
        }
    }
}