using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables.ValueReferences
{
    [InlineProperty]
    [SynchronizedHeader]
    [HideReferenceObjectPicker]
    public class ValueReference<I, T> where I : class, IValue<T>
    {
        #region Inspector

        public String LabelText => UseConstant ? "" : "?";
        public String Tooltip => ReferenceValue != null && !UseConstant ? ReferenceValue.GetDescription() : "";
        
        [HorizontalGroup("Split", LabelWidth = 0.001f)]
        [HorizontalGroup("Split/Left", LabelWidth = .01f, Width = .1f)]
        [BoxGroup("Split/Left/Left", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        [LabelText("$LabelText")]
        [LabelWidth(10f)]
        [SerializeField]
        protected bool UseConstant = false;

        [ValueDropdown("GetTypes")]
        [HorizontalGroup("Split/Left/Right", LabelWidth = .01f, Width = .75f)]
        [BoxGroup("Split/Left/Right/Right", ShowLabel = false)]
        [HideLabel]
        [LabelWidth(.01f)]
        [SerializeField] 
        protected Type InstanceType;

        [HorizontalGroup("Split", LabelWidth = 0.001f, Width = .5f)]
        [BoxGroup("Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("UseConstant")]
        [SerializeField]
        protected T ConstantValue;
        
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
                    return $"{ConstantValue}";
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

        private static IEnumerable GetTypes()
        {
            var type = typeof(I);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
            return types;
        }

        [PropertyTooltip("Create an Instance SO")]
        [BoxGroup("Split/Left/Right/Left", ShowLabel = false)] 
        [LabelWidth(.01f)]
        [Button("I", ButtonSizes.Small)]
        public void CreateInstance()
        {
            UseConstant = false;
            ReferenceValue = ScriptableObject.CreateInstance(InstanceType) as I;
        }

        #endregion
    }
}