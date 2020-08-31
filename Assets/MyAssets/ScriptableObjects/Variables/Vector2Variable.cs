using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    // [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "Vector2Variable", menuName = "Variables/Vector2Variable", order = 0)]
    public class Vector2Variable : Variable
    {
        [SerializeField] private Vector2 defaultValue;
        [SerializeField] private Vector2 runtimeValue;

        public Vector2 Value
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
    public class Vector2Reference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] private Vector2 ConstantValue;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")]
        [SerializeField] private Vector2Variable Variable;

        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";

        public Vector2 Value
        {
            get => UseConstant ? ConstantValue : Variable.Value;
            set => Variable.Value = value;
        }
    }
}