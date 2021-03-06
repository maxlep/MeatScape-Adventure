﻿using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;
using UnityEngine.Assertions;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will instantiate a particle system and play/stop it when playing/stopping the feedback
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will instantiate the specified ParticleSystem at the specified position on Start or on Play, optionally nesting them.")]
    [FeedbackPath("Particles/Particles Spawn At Position")]
    public class MMFeedbackSpawnParticlesAtPosition : MMFeedback
    {
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
        #endif
        
        public GameObject ParticlesPrefab;
        
        public Vector3Reference TargetPosition;
        public Vector3 Offset;
        public float DestroyTime = 5f;

        protected ParticleSystem _instantiatedParticleSystem;
        

        /// <summary>
        /// Instantiates the particle system
        /// </summary>
        protected virtual void InstantiateParticleSystem()
        {
            _instantiatedParticleSystem =
                (Instantiate(ParticlesPrefab, TargetPosition.Value + Offset, Quaternion.identity))
                .GetComponent<ParticleSystem>();
        }
        

        /// <summary>
        /// On Play, plays the feedback
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            InstantiateParticleSystem();
            _instantiatedParticleSystem.Play();
            Destroy(_instantiatedParticleSystem, DestroyTime);
        }

        /// <summary>
        /// On Stop, stops the feedback
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomStopFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (_instantiatedParticleSystem != null)
                _instantiatedParticleSystem.Stop();
        }
        
    }
}
