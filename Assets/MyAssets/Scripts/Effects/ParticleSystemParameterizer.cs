using System;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.Effects
{
    public class ParticleSystemParameterizer : SerializedMonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        
        [SerializeField] private FloatValueReference _startSpeed;
        [SerializeField] private FloatValueReference _startLifetimeMultiplier;
        [SerializeField] private FloatValueReference _startSizeMultiplier;

        private void Awake()
        {
            // var particleSystemMain = _particleSystem.main;
            //
            // if (_startSpeed?.Value != null)
            // {
            //     particleSystemMain.startSpeed = new ParticleSystem.MinMaxCurve(_startSpeed.Value);
            //     _startSpeed.Subscribe(() =>
            //     {
            //         var particleSystemMain = _particleSystem.main;
            //         particleSystemMain.startSpeed = new ParticleSystem.MinMaxCurve(_startSpeed.Value);
            //     });
            // }
            //
            // if (_startLifetimeMultiplier?.Value != null)
            // {
            //     particleSystemMain.startLifetimeMultiplier = _startLifetimeMultiplier.Value;
            //     _startLifetimeMultiplier.Subscribe(() =>
            //     {
            //         var particleSystemMain = _particleSystem.main;
            //         particleSystemMain.startLifetimeMultiplier = _startLifetimeMultiplier.Value;
            //     });
            // }
            //
            // if (_startSizeMultiplier?.Value != null)
            // {
            //     var origStartSize = particleSystemMain.startSize;
            //     particleSystemMain.startSize = new ParticleSystem.MinMaxCurve
            //     (
            //         origStartSize.constantMin * _startSizeMultiplier.Value,
            //         origStartSize.constantMax * _startSizeMultiplier.Value
            //     );
            //     _startSizeMultiplier.Subscribe(() =>
            //     {
            //         var particleSystemMain = _particleSystem.main;
            //         particleSystemMain.startSize = new ParticleSystem.MinMaxCurve
            //         (
            //             origStartSize.constantMin * _startSizeMultiplier.Value,
            //             origStartSize.constantMax * _startSizeMultiplier.Value
            //         );
            //     });
            // }
        }
    }
}