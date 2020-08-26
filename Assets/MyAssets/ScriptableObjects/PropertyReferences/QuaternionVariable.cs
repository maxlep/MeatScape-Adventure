using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "QuaternionVariable", menuName = "Variables/QuaternionVariable", order = 0)]
public class QuaternionVariable : ScriptableObject
{
    [SerializeField] private Quaternion defaultValue;
    [SerializeField] private Quaternion runtimeValue;
    
    public Quaternion Value
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
public class QuaternionReference
{
    [HorizontalGroup("Split", LabelWidth = .09f)] [Tooltip("Use Constant or VariableReference")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelWidth(.01f)]
    public bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    public Quaternion ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")]  
    public QuaternionVariable Variable;

    public Quaternion Value => UseConstant ? ConstantValue : Variable.Value;
}