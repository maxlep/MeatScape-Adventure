using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Required]
[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "FloatVariable", menuName = "Variables/FloatVariable", order = 0)]
public class FloatVariable : ScriptableObject
{
    [SerializeField] private float defaultValue;
    [SerializeField] private float runtimeValue;
    [TextArea] [HideInInlineEditors] public String Description;
    
    public delegate void OnUpdate_();
    public event OnUpdate_ OnUpdate;
    
    public float Value
    {
        get => runtimeValue;
        set
        {
            runtimeValue = value;
            OnUpdate?.Invoke();
        }
    }

    private void OnEnable() => runtimeValue = defaultValue;
    
}

[Serializable]
[InlineProperty]
public class FloatReference
{
    [HorizontalGroup("Split", LabelWidth = .01f)] [PropertyTooltip("$Tooltip")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
    [SerializeField] private bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    [SerializeField] private float ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    [SerializeField] private FloatVariable Variable;
    
    public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
    public String LabelText => UseConstant ? "" : "?";

    public float Value
    {
        get => UseConstant ? ConstantValue : Variable.Value;
        set => Variable.Value = value;
    }
}