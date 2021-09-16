using Sirenix.OdinInspector;
using Unity.Properties;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// this feedback will let you animate the position of 
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will set the parent of a transform or remove if null")]
    [FeedbackPath("Transform/SetParent")]
    public class MMFeedbackSetParent : MMFeedback
    {
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
        #endif

        [Header("Transform")]
        [SerializeField] private bool useSceneReferenceTarget = false;
        [SerializeField] [ShowIf("useSceneReferenceTarget")] private TransformSceneReference TargetRef;
        [SerializeField] [HideIf("useSceneReferenceTarget")] private Transform Target;
        
        [SerializeField] private bool useSceneReferenceParent = false;
        [SerializeField] [ShowIf("useSceneReferenceParent")] private TransformSceneReference NewParentRef;
        [SerializeField] [HideIf("useSceneReferenceParent")] private Transform NewParent;
        

        /// <summary>
        /// On init, we set our initial and destination positions (transform will take precedence over vector3s)
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
        }
        
        /// <summary>
        /// On Play, we move our object from A to B
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            Transform targetTrans = (useSceneReferenceTarget) ? TargetRef.Value : Target;
            if (targetTrans == null) return;
            
            Transform targetParent = (useSceneReferenceParent) ? NewParentRef.Value : NewParent;

            targetTrans.parent = targetParent;
        }

        
        
    }
}