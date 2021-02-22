using System;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "EvaluateCurveVariable", menuName = "Variables/EvaluateCurveVariable", order = 0)]
    public class EvaluateCurveVariable : SerializedScriptableObject, IFloatValue
    {
        #region Inspector
        
        [TextArea (7, 10)]
        [HideInInlineEditors]
        public string Description;
        
        [SerializeField] private CurveReference _curve;
        [SerializeField] private FloatValueReference _x;
        
        #endregion
        
        public float Value
        {
            get
            {
                return _curve.Value.Evaluate(_x.Value);
            }
        }
        
        #region Interface
        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetValue(Type type) => Value;
        public float GetFloat() => Value;

        public void Subscribe(OnUpdate callback)
        {
            _curve.Subscribe(callback);
            _x.Subscribe(callback);
        }
        
        public void Unsubscribe(OnUpdate callback)
        {
            _curve.Unsubscribe(callback);
            _x.Unsubscribe(callback);
        }
        
        #endregion
    }
}