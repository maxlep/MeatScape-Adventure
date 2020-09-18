using System;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

namespace MyAssets.Scripts.ShaderHelpers
{
    public class ShadowProjectorSetter : MonoBehaviour
    {
        [SerializeField] private FloatVariable blobDiameter;
        [SerializeField] private FloatReference heightToHalf;
        [SerializeField] private Projector projector;

        private void Awake()
        {
            UpdateProjector();
            blobDiameter.Subscribe(UpdateProjector);
        }

        public void UpdateProjector()
        {
            if (projector.orthographic)
            {
                projector.orthographicSize = blobDiameter.Value;
                Debug.Log($"Set orthographic size {blobDiameter.Value} {projector.orthographicSize}");
            }
            else
            {
                // var angle = projector.fieldOfView / 2;
                // var radius = blobDiameter.Value / 2;
                // var distance = radius / Mathf.Tan(angle * Mathf.Deg2Rad);
                //
                // var projectorTransform = projector.transform;
                // var projectorLocalPosition = projectorTransform.localPosition;
                // var newPosition = new Vector3(projectorLocalPosition.x, -distance, projectorLocalPosition.z);
                // projectorTransform.localPosition = newPosition;
                // projector.farClipPlane = distance + blobDiameter.Value;
                // Debug.Log($"Set distance {blobDiameter.Value} {distance} {newPosition} {projectorLocalPosition}");
                var radius = blobDiameter.Value / 3;
                var angle = Mathf.Atan2(radius, heightToHalf.Value) * Mathf.Rad2Deg;
                var height = 2 * heightToHalf.Value;

                var projectorTransform = projector.transform;
                var projectorLocalPosition = projectorTransform.localPosition;
                var newPosition = new Vector3(projectorLocalPosition.x, -height, projectorLocalPosition.z);
                projectorTransform.localPosition = newPosition;
                projector.fieldOfView = 2 * angle;
                projector.farClipPlane = height + blobDiameter.Value;
            }
        }
    }
}