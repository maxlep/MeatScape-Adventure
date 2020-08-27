using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Required]
[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/IntVariable", order = 0)]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int defaultValue;
    [SerializeField] private int runtimeValue;
    [TextArea] [HideInInlineEditors] public String Description;
    
    public delegate void OnUpdate_();
    public event OnUpdate_ OnUpdate;
    
    public int Value
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
public class IntReference
{
    [HorizontalGroup("Split", LabelWidth = .01f)] [PropertyTooltip("$Tooltip")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
    [SerializeField] private bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    [SerializeField] private int ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    [SerializeField] private IntVariable Variable;
    
    public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
    public String LabelText => UseConstant ? "" : "?";

    public int Value
    {
        get => UseConstant ? ConstantValue : Variable.Value;
        set => Variable.Value = value;
    }
}
