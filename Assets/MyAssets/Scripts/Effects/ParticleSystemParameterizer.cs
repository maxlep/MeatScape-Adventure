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
        
        [SerializeField] private FloatValueReference _startSpeedMultiplier;
        [SerializeField] private FloatValueReference _startLifetimeMultiplier;

        private void Awake()
        {
            // var particleSystemMain = _particleSystem.main;
            // particleSystemMain.startLifetimeMultiplier = _startLifetimeMultiplier.Value;
            // particleSystemMain.startSpeed = new ParticleSystem.MinMaxCurve(_startSpeedMultiplier.Value);
            //
            // _startSpeedMultiplier?.Subscribe(() =>
            // {
            //     var particleSystemMain = _particleSystem.main;
            //     particleSystemMain.startSpeed = new ParticleSystem.MinMaxCurve(_startSpeedMultiplier.Value);
            // });
            // _startLifetimeMultiplier?.Subscribe(() =>
            // {
            //     var particleSystemMain = _particleSystem.main;
            //     particleSystemMain.startLifetimeMultiplier = _startLifetimeMultiplier.Value;
            // });
        }
    }
}