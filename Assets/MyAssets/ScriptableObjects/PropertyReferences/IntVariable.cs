using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/IntVariable", order = 0)]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int defaultValue;
    [SerializeField] private int runtimeValue;
    
    public int Value
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
public class IntReference
{
    [HorizontalGroup("Split", LabelWidth = .09f)] [Tooltip("Use Constant or VariableReference")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelWidth(.01f)]
    public bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    public int ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    public IntVariable Variable;

    public int Value => UseConstant ? ConstantValue : Variable.Value;
}
