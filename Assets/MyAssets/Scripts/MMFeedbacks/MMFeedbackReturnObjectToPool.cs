using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will instantiate the associated object (usually a VFX, but not necessarily), optionnally creating an object pool of them for performance
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback returns an object to the pool manager.")]
    [FeedbackPath("GameObject/Return Object To Pool")]
    public class MMFeedbackReturnObjectToPool : MMFeedback
    {
        /// the different ways to position the instantiated object :
        /// - FeedbackPosition : object will be instantiated at the position of the feedback, plus an optional offset
        /// - Transform : the object will be instantiated at the specified Transform's position, plus an optional offset
        /// - WorldPosition : the object will be instantiated at the specified world position vector, plus an optional offset
        /// - Script : the position passed in parameters when calling the feedback

        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        #endif

        [Header("Return Object")]
        /// the vfx object to instantiate
        public GameObject ObjectToPool;
        
        
        /// <summary>
        /// On Play we instantiate the specified object, either from the object pool or from scratch
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (!ObjectPoolManager.Instance.ReturnObjectToPool(ObjectToPool))
                Debug.LogError($"Failed to return object {ObjectToPool.name} to the pool!");
        }
    }
}