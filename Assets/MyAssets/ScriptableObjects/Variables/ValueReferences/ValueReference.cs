using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Linq;
using MoreMountains.Tools;
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
        [OnValueChanged("ResetInstanceOptions")]
        [SerializeField]
        protected bool EnableInstanceOptions = false;

        [VerticalGroup("Middle")]
        [VerticalGroup("Middle/Box/Top")]
        [LabelText("Name")]
        [LabelWidth(40f)]
        [ShowIf("InstanceNotConstant")]
        [SerializeField] 
        protected String InstanceName = "{I} ";

        [VerticalGroup("Middle/Box/Middle")]
        [LabelText("Folder")]
        [LabelWidth(40f)]
        [FolderPath(ParentFolder = "Assets/MyAssets/ScriptableObjects/", RequireExistingPath = true)]
        [ShowIf("InstanceNotConstant")]
        [SerializeField] 
        protected String InstancePath = "IntermediateProperties";

        [ValueDropdown("GetTypes")]
        [BoxGroup("Middle/Box", ShowLabel = false)]
        [VerticalGroup("Middle/Box/Middle2")]
        [LabelText("Type")]
        [LabelWidth(40f)]
        [ShowIf("InstanceNotConstant")]
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

        protected bool InstanceNotConstant => (!UseConstant && EnableInstanceOptions);
        
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
        [VerticalGroup("Middle/Box/Bottom")]
        [LabelWidth(.01f)]
        [GUIColor(.85f, 1f, .9f)]
        [ShowIf("InstanceNotConstant")]
        [Button("Create Instance", ButtonSizes.Small)]
        public void CreateInstance()
        {
            UseConstant = false;
            I tempScriptableObj = ScriptableObject.CreateInstance(InstanceType) as I;
            String fullPath = $"Assets/MyAssets/ScriptableObjects/{InstancePath}/";

            //Try saving scriptable object to desired path
            if (tempScriptableObj.Save(fullPath, InstanceName))
            {
                ReferenceValue = tempScriptableObj;
                EnableInstanceOptions = false;
            }
        }

        private void ResetInstanceOptions()
        {
            InstanceName = "{I} ";
            InstancePath = "IntermediateProperties";
        }

        #endregion
    }
}