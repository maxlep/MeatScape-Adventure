using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    // [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "Variables/BoolVariable", order = 0)]
    public class BoolVariable : Variable
    {
        [SerializeField] private bool defaultValue;
        [SerializeField] private bool runtimeValue;

        public bool Value
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
    public class BoolReference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [LabelText("Value")] [ShowIf("UseConstant")]
        [SerializeField] private bool ConstantValue;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")]
        [SerializeField] private BoolVariable Variable;
    
        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
    
        public bool Value
        {
            get => UseConstant ? ConstantValue : Variable.Value;
            set => Variable.Value = value;
        }
    }
}