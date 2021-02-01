using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables.ValueReferences
{
    [LabelWidth(100)]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public class ValueReference<I, T> where I : IValue<T>
    {
        #region Inspector

        public String LabelText => UseConstant ? "" : "?";
        public String Tooltip => ReferenceValue != null && !UseConstant ? ReferenceValue.GetDescription() : "";
        
        [HorizontalGroup("Split", LabelWidth = 0.001f)]
        [BoxGroup("Split/Left", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        [LabelText("$LabelText")]
        [LabelWidth(10f)]
        [SerializeField]
        protected bool UseConstant = false;

        [HorizontalGroup("Split", LabelWidth = 0.001f)]
        [BoxGroup("Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("UseConstant")]
        [SerializeField]
        protected T ConstantValue;

        [HorizontalGroup("Split", LabelWidth = 0.001f)]
        [BoxGroup("Split/Right", ShowLabel = false)]
        [HideLabel]
        [HideIf("UseConstant")]
        [InlineEditor()]
        [SerializeField]
        protected I ReferenceValue;

        #endregion
        
        #region Interface

        public T Value
        {
            get
            {
                if (UseConstant)
                    return ConstantValue;
                if (ReferenceValue != null)
                    return ReferenceValue.GetValue(typeof(T));
                Debug.LogError($"Trying to access Float variable but none set in inspector!");
                return default(T);
            }
        }

        public String Name
        {
            get
            {
                if (UseConstant)
                    return $"<Const>{ConstantValue}";
                if (ReferenceValue != null)
                    return ReferenceValue.GetName();
                return "<Missing Float>";
            }
        }

        public void Subscribe(OnUpdate callback)
        {
            ReferenceValue?.Subscribe(callback);
        }

        public void Unsubscribe(OnUpdate callback)
        {
            ReferenceValue?.Unsubscribe(callback);
        }

        #endregion
    }
}