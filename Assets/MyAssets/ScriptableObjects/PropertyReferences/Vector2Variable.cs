using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "Vector2Variable", menuName = "Variables/Vector2Variable", order = 0)]
public class Vector2Variable : ScriptableObject
{
    [SerializeField] private Vector2 defaultValue;
    [SerializeField] private Vector2 runtimeValue;
    [TextArea] [HideInInlineEditors] public String Description;

    public Vector2 Value
    {
        get => runtimeValue;
        set => runtimeValue = value;
    } 

    private void OnEnable() => runtimeValue = defaultValue;

}

[Serializable]
[InlineProperty]
public class Vector2Reference
{
    [HorizontalGroup("Split", LabelWidth = .01f)] [PropertyTooltip("$Tooltip")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
    [SerializeField] private bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    [SerializeField] private Vector2 ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")]
    [SerializeField] private Vector2Variable Variable;

    public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
    public String LabelText => UseConstant ? "" : "?";

    public Vector2 Value
    {
        get => UseConstant ? ConstantValue : Variable.Value;
        set => Variable.Value = value;
    }
}
