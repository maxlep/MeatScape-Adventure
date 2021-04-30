using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables.ValueReferences
{
    [InlineProperty]
    [SynchronizedHeader]
    [HideReferenceObjectPicker]
    public class ValueReference<I, T> 
        where I : class, IValue<T>
    {
        #region Inspector
        
        public String Tooltip => ReferenceValue != null && !UseConstant ? ReferenceValue.GetDescription() : "";
        
        [VerticalGroup("Top")]
        [HorizontalGroup("Top/Split", LabelWidth = 0.001f)]
        [HorizontalGroup("Top/Split/Left", LabelWidth = .01f)]
        [BoxGroup("Top/Split/Left/Left", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        [LabelText("C")]
        [LabelWidth(10f)]
        [SerializeField]
        protected bool UseConstant = false;
        
        [HorizontalGroup("Top/Split/Left/Right", LabelWidth = .01f)]
        [BoxGroup("Top/Split/Left/Right/Right", ShowLabel = false)]
        [PropertyTooltip("$Tooltip")]
        [LabelText("I")]
        [LabelWidth(10f)]
        [SerializeField]
        protected bool EnableInstanceOptions = false;

        [VerticalGroup("Middle")]
        [VerticalGroup("Middle/Box/Bottom")]
        [LabelText("Name")]
        [LabelWidth(40f)]
        [ShowIf("EnableInstanceOptions")]
        [SerializeField] 
        protected String InstanceName;

        [ValueDropdown("GetTypes")]
        [BoxGroup("Middle/Box", ShowLabel = false)]
        [HorizontalGroup("Middle/Box/Bottom/Split", LabelWidth = 0.001f)]
        [HideLabel]
        [LabelWidth(.01f)]
        [ShowIf("EnableInstanceOptions")]
        [SerializeField] 
        protected Type InstanceType;

        [HorizontalGroup("Top/Split", LabelWidth = 0.001f, Width = .7f)]
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
        [HideLabel]
        [ShowIf("UseConstant")]
        [SerializeField]
        protected T ConstantValue;
        
        [BoxGroup("Top/Split/Right", ShowLabel = false)]
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
        [HorizontalGroup("Middle/Box/Bottom/Split/Right", LabelWidth = 0.001f)]
        [LabelWidth(.01f)]
        [GUIColor(.85f, 1f, .9f)]
        [ShowIf("EnableInstanceOptions")]
        [Button("Create Instance", ButtonSizes.Small)]
        public void CreateInstance()
        {
            UseConstant = false;
            ReferenceValue = ScriptableObject.CreateInstance(InstanceType) as I;
            ReferenceValue.Save(InstanceName);
            EnableInstanceOptions = false;
        }

        #endregion
    }
}