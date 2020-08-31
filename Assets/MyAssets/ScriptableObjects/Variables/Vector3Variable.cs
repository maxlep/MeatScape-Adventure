using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "Vector3Variable", menuName = "Variables/Vector3Variable", order = 0)]
    public class Vector3Variable : Variable
    {
        [SerializeField] private Vector3 defaultValue;
        [SerializeField] private Vector3 runtimeValue;
    
        public Vector3 Value
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
    public class Vector3Reference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] private Vector3 ConstantValue;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
        [SerializeField] private Vector3Variable Variable;
    
        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";

        public Vector3 Value
        {
            get => UseConstant ? ConstantValue : Variable.Value;
            set => Variable.Value = value;
        }
    }
}