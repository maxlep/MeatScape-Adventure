using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will instantiate the associated object (usually a VFX, but not necessarily), optionnally creating an object pool of them for performance
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to instantiate object from pooler specified in its inspector, at the feedback's position (plus an optional offset). You can also optionally (and automatically) create an object pool at initialization to save on performance. In that case you'll need to specify a pool size (usually the maximum amount of these instantiated objects you plan on having in your scene at each given time).")]
    [FeedbackPath("GameObject/Instantiate Object From Pool")]
    public class MMFeedbackInstantiateObjectFromPool : MMFeedback
    {
        /// the different ways to position the instantiated object :
        /// - FeedbackPosition : object will be instantiated at the position of the feedback, plus an optional offset
        /// - Transform : the object will be instantiated at the specified Transform's position, plus an optional offset
        /// - WorldPosition : the object will be instantiated at the specified world position vector, plus an optional offset
        /// - Script : the position passed in parameters when calling the feedback
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        #endif

        [Header("Instantiate Object")]
        /// the vfx object to instantiate
        public GameObject VfxToInstantiate;

        [Header("Position")]
        /// the chosen way to position the object 
        public PositionModes PositionMode = PositionModes.FeedbackPosition;
        /// the transform at which to instantiate the object
        [MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
        public Transform TargetTransform;
        /// the transform at which to instantiate the object
        [MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
        public Vector3 TargetPosition;
        /// the position offset at which to instantiate the vfx object
        public Vector3 VfxPositionOffset;

        protected GameObject _newGameObject;
        
        /// <summary>
        /// On Play we instantiate the specified object, either from the object pool or from scratch
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
        {
            if (Active && (VfxToInstantiate != null))
            {
                _newGameObject = ObjectPoolManager.Instance.GetObjectFromPool(VfxToInstantiate, GetPosition(position),
                    Quaternion.identity);
                //_newGameObject = GameObject.Instantiate(VfxToInstantiate) as GameObject;
                //_newGameObject.transform.position = GetPosition(position);
                
            }
        }

        protected virtual Vector3 GetPosition(Vector3 position)
        {
            switch (PositionMode)
            {
                case PositionModes.FeedbackPosition:
                    return this.transform.position + VfxPositionOffset;
                case PositionModes.Transform:
                    return TargetTransform.position + VfxPositionOffset;
                case PositionModes.WorldPosition:
                    return TargetPosition + VfxPositionOffset;
                case PositionModes.Script:
                    return position + VfxPositionOffset;
                default:
                    return position + VfxPositionOffset;
            }
        }
    }
}