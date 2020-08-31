using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    // [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "Variables/FloatVariable", order = 0)]
    public class FloatVariable : Variable
    {
        [SerializeField] private float defaultValue;
        [SerializeField] private float runtimeValue;
    
        public float Value
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
    public class FloatReference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] private float ConstantValue;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
        [SerializeField] private FloatVariable Variable;
    
        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";

        public float Value
        {
            get => UseConstant ? ConstantValue : Variable.Value;
            set => Variable.Value = value;
        }
    }
}