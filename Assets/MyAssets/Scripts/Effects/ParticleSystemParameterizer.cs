using System;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

namespace MyAssets.Scripts.Effects
{
    public class ParticleSystemParameterizer : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        
        [SerializeField] private FloatReference _startSpeedMultiplier;
        [SerializeField] private FloatReference _startLifetimeMultiplier;

        private void Awake()
        {
            _startSpeedMultiplier?.Subscribe(() =>
            {
                var particleSystemMain = _particleSystem.main;
                particleSystemMain.startSpeed = _startSpeedMultiplier.Value;
                particleSystemMain.startLifetimeMultiplier = _startLifetimeMultiplier.Value;
            });
        }
    }
}