using System;
using Cinemachine;
using EnhancedHierarchy;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.Camera
{
    public class CameraRigScaler : SerializedMonoBehaviour
    {
        [SerializeField] private CinemachineFreeLook _cinemachineFreeLook;

        [SerializeField] private FloatReference _scaleValue;
        [SerializeField] private FloatReference _scaleMin;
        [SerializeField] private FloatReference _scaleMax;

        [SerializeField] private CurveReference _topRigRadius;
        [SerializeField] private CurveReference _topRigHeight;
        [SerializeField] private CurveReference _midRigRadius;
        [SerializeField] private CurveReference _midRigHeight;
        [SerializeField] private CurveReference _botRigRadius;
        [SerializeField] private CurveReference _botRigHeight;

        private bool doUpdate = false;

        private void Start()
        {
            _scaleValue.Subscribe(() =>
            {
                doUpdate = true;
            });
        }

        private void LateUpdate()
        {
            if (doUpdate)
            {
                doUpdate = false;
                
                var fac = (_scaleValue.Value - _scaleMin.Value) /
                          (_scaleMax.Value - _scaleMin.Value);
                
                _cinemachineFreeLook.m_Orbits[0] = new CinemachineFreeLook.Orbit(_topRigHeight.Value.Evaluate(fac), _topRigRadius.Value.Evaluate(fac));
                _cinemachineFreeLook.m_Orbits[1] = new CinemachineFreeLook.Orbit(_midRigHeight.Value.Evaluate(fac), _midRigRadius.Value.Evaluate(fac));
                _cinemachineFreeLook.m_Orbits[2] = new CinemachineFreeLook.Orbit(_botRigHeight.Value.Evaluate(fac), _botRigRadius.Value.Evaluate(fac));
            }
        }
    }
}