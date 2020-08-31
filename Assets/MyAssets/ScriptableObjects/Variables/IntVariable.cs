using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/IntVariable", order = 0)]
    public class IntVariable : Variable
    {
        [SerializeField] private int defaultValue;
        [SerializeField] private int runtimeValue;
    
        public int Value
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
    public class IntReference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] private int ConstantValue;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
        [SerializeField] private IntVariable Variable;
    
        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";

        public int Value
        {
            get => UseConstant ? ConstantValue : Variable.Value;
            set => Variable.Value = value;
        }
    }
}