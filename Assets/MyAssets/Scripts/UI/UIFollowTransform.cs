using System;
using MyAssets.ScriptableObjects.Events;
using MyAssets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MyAssets.Scripts.UI
{
    [ExecuteAlways]
    public class UIFollowTransform : MonoBehaviour
    {
        [SerializeField] private GameEvent _onPreCull;
        [SerializeField] private TransformSceneReference _cameraTransform;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasScaler _canvasScaler;
        [SerializeField] private RectTransform _follower;
        [SerializeField] private TransformSceneReference _followed;
        [SerializeField] private bool _pauseFollow;

        private UnityEngine.Camera _camera;
        private Vector3 _followerOriginalPosition;

        private void Awake()
        {
            _followerOriginalPosition = _follower.position;
        }

        private void OnValidate()
        {
            _camera = _cameraTransform.Value.GetComponent<UnityEngine.Camera>();
        }

        private void OnEnable()
        {
            _onPreCull.Subscribe(RunUpdate);
        }

        private void OnDisable()
        {
            _onPreCull.Unsubscribe(RunUpdate);
        }

        private void RunUpdate()
        {
            if (_pauseFollow)
            {
                _followerOriginalPosition = _follower.position;
            }
            else if (_follower != null && _followed != null && _followed.Value != null && _camera != null && _canvas != null)
            {
                var worldPoint = _followed.Value.position;
                var screenPoint = _camera.WorldToScreenPoint(worldPoint).xy();
                var canvasRect = _canvas.transform as RectTransform;
                if (canvasRect != null)
                {
                    var canvasPoint = screenPoint * _camera.pixelRect.size.Inverse() * canvasRect.rect.size;
                    _follower.anchoredPosition = canvasPoint;
                }
            }
            else if (_follower != null)
            {
                _follower.position = _followerOriginalPosition;
            }
        }
    }
}