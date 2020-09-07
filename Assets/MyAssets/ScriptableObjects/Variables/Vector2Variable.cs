﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    // [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "Vector2Variable", menuName = "Variables/Vector2Variable", order = 0)]
    public class Vector2Variable : Variable
    {
        [SerializeField] [LabelWidth(110f)] private Vector2 defaultValue;
        [SerializeField] [LabelWidth(110f)] private Vector2 runtimeValue;

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
            get
            {
                if (Variable != null)
                    return UseConstant ? ConstantValue : Variable.Value;
                else
                {
                    if (UseConstant)
                        return ConstantValue;
                    else
                    {
                        Debug.LogError("Trying to access Vector2 variable but none set in inspector!");
                        return Vector2.zero;
                    }
                }
            }
            set
            {
                if (Variable != null) Variable.Value = value;
            } 
        }
    }
}