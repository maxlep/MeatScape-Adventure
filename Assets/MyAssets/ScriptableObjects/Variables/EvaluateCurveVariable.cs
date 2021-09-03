using System;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEditor;
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
        
        [SerializeField] [LabelWidth(120f)] private bool _evaluateAsPercent = false;

        [ShowIf("_evaluateAsPercent"), SerializeField] private bool _useZeroBaseline;
        [SerializeField] [LabelWidth(50f)] private CurveReference _curve;
        [SerializeField] [LabelWidth(55f)] private FloatValueReference _x;

        [ShowInInspector] [LabelWidth(50f)] private float _value;
        #endregion
        
        protected event OnUpdate OnUpdate;

        [ReadOnly] public float Value => _value;
        
        [Button]
        private void Recalculate()
        {
            _value = _evaluateAsPercent
                ? _curve.EvaluateFactor(_x.Value, _useZeroBaseline)
                : _curve.Value.Evaluate(_x.Value);
            OnUpdate?.Invoke();
        }

#region Interface
        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetValue(Type type) => Value;
        public float GetFloat() => Value;

        public void Subscribe(OnUpdate callback)
        {
            OnUpdate += callback;
        }
        
        public void Unsubscribe(OnUpdate callback)
        {
            OnUpdate -= callback;
        }

        public bool Save(string folderPath, string name = "")
        { 
            return this.SaveInstance(folderPath, name);
        }

        #endregion
        
        #region Lifecycle
        private void OnEnable()
        {
            Recalculate();
            _curve.Subscribe(Recalculate);
            _x.Subscribe(Recalculate);
        }

        private void OnDisable()
        {
            _curve.Unsubscribe(Recalculate);
            _x.Unsubscribe(Recalculate);
        }
        #endregion
    }
}