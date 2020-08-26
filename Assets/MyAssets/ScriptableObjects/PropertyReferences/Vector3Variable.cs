using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "Vector3Variable", menuName = "Variables/Vector3Variable", order = 0)]
public class Vector3Variable : ScriptableObject
{
    [SerializeField] private Vector3 defaultValue;
    [SerializeField] private Vector3 runtimeValue;
    [TextArea] [HideInInlineEditors] public String Description;
    
    public Vector3 Value
    {
        get => runtimeValue;
        set => runtimeValue = value;
    } 
    private void OnEnable()
    {
        runtimeValue = defaultValue;
    }}

[Serializable]
[InlineProperty]
public class Vector3Reference
{
    [HorizontalGroup("Split", LabelWidth = .01f)] [PropertyTooltip("$Tooltip")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
    [SerializeField] private bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    [SerializeField] private Vector3 ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    [SerializeField] private Vector3Variable Variable;
    
    public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
    public String LabelText => UseConstant ? "" : "?";

    public Vector3 Value
    {
        get => UseConstant ? ConstantValue : Variable.Value;
        set => Variable.Value = value;
    }
}
