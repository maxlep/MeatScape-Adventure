using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will cause a pause when met, preventing any other feedback lower in the sequence to run until it's complete.
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will cause a random pause when met, preventing any other feedback lower in the sequence to run until it's complete.")]
    [FeedbackPath("Pause/RandomPause")]
    public class MMFeedbackRandomPause : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PauseColor; } }
        #endif
        public override YieldInstruction Pause { get { return _waitForSeconds; } }
        
        [Header("Pause")]
        /// the duration of the pause, in seconds.
        public float PauseDurationMin = 1f;
        public float PauseDurationMax = 2f;
        
        /// the duration of this feedback is the duration of the pause
        public override float FeedbackDuration { get { return pauseDuration; } }

        protected float pauseDuration;
        protected WaitForSeconds _waitForSeconds;

        /// <summary>
        /// On init we cache our wait for seconds
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            pauseDuration = Random.Range(PauseDurationMin, PauseDurationMax);
            base.CustomInitialization(owner);
            CacheWaitForSeconds();
        }

        /// <summary>
        /// On play we trigger our pause
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active)
            {
                pauseDuration = Random.Range(PauseDurationMin, PauseDurationMax);
                StartCoroutine(PlayPause());
            }
        }

        /// <summary>
        /// Pause coroutine
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PlayPause()
        {
            yield return Pause;
        }

        /// <summary>
        /// Caches our waitforseconds
        /// </summary>
        protected virtual void CacheWaitForSeconds()
        {
            _waitForSeconds = new WaitForSeconds(pauseDuration);
        }

        /// <summary>
        /// When changed, we cache our waitforseconds again
        /// </summary>
        protected virtual void OnValidate()
        {
            CacheWaitForSeconds();
        }
    }
}
