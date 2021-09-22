using System;
using UnityEngine;

namespace MyAssets.Scripts.Camera
{
    public class CameraPlayerTransparencyController : MonoBehaviour
    {
        [SerializeField] private LayerMapper LayerMapper;
        [SerializeField] private SkinnedMeshRenderer PlayerMeshRenderer;
        [SerializeField] private float TransparentAlpha = .1f;
        [SerializeField] private float OpaqueAlpha = 1f;
        [SerializeField] private float LerpTime = .2f;

        private Material playerMat;
        private LTDescr StartTransparencyTween;
        private LTDescr StopTransparencyTween;
        private const string ALBEDO_COLOR = "_AlbedoColor";

        private void Awake()
        {
            playerMat = PlayerMeshRenderer.material;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != LayerMapper.GetLayer(LayerEnum.Camera))
                return;
            
            //Cancel stop tween if running
            if (StopTransparencyTween != null) LeanTween.cancel(StopTransparencyTween.id);
            
            //Start tween to transparent
            Color currentAlbedo = playerMat.GetColor(ALBEDO_COLOR);
            StartTransparencyTween = LeanTween.value(currentAlbedo.a, TransparentAlpha, LerpTime);
            StartTransparencyTween.setOnUpdate(a =>
            {
                Color newAlbedo = new Color(currentAlbedo.r, currentAlbedo.g, currentAlbedo.b, a);
                playerMat.SetColor(ALBEDO_COLOR, newAlbedo);
            });


        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != LayerMapper.GetLayer(LayerEnum.Camera))
                return;

            //Cancel start tween if running
            if (StartTransparencyTween != null) LeanTween.cancel(StartTransparencyTween.id);
            
            //Start tween to opaque
            Color currentAlbedo = playerMat.GetColor(ALBEDO_COLOR);
            StopTransparencyTween = LeanTween.value(currentAlbedo.a, OpaqueAlpha, LerpTime);
            StopTransparencyTween.setOnUpdate(a =>
            {
                Color newAlbedo = new Color(currentAlbedo.r, currentAlbedo.g, currentAlbedo.b, a);
                playerMat.SetColor(ALBEDO_COLOR, newAlbedo);
            });
            
        }
    }
}