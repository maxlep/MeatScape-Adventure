using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    // [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "QuaternionVariable", menuName = "Variables/QuaternionVariable", order = 0)]
    public class QuaternionVariable : Variable
    {
        [SerializeField] [LabelWidth(110f)] private Quaternion defaultValue;
        [SerializeField] [LabelWidth(110f)] private Quaternion runtimeValue;
    
        public Quaternion Value
        {
            get => runtimeValue;
            set
            {
                runtimeValue = value;
                base.BroadcastUpdate();
            }
        }

        private void OnEnable() => runtimeValue = defaultValue;
    
    }

    [Serializable]
    [InlineProperty]
    public class QuaternionReference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] private Quaternion ConstantValue;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
        [SerializeField] private QuaternionVariable Variable;
    
        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";

        public Quaternion Value
        {
            get => UseConstant ? ConstantValue : Variable.Value;
            set
            {
                if (Variable != null) Variable.Value = value;
            } 
        }
    }
}