﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "BoolVariable", menuName = "Variables/BoolVariable", order = 0)]
public class BoolVariable : ScriptableObject
{
    [SerializeField] private bool defaultValue;
    [SerializeField] private bool runtimeValue;
    
    public bool Value
    {
        get => runtimeValue;
        set => runtimeValue = value;
    } 
    private void OnEnable()
    {
        runtimeValue = defaultValue;
    }
}

[Serializable]
[InlineProperty]
public class BoolReference
{
    [HorizontalGroup("Split", LabelWidth = .09f)] [Tooltip("Use Constant or VariableReference")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelWidth(.01f)]
    [SerializeField] private bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [LabelText("Value")] [ShowIf("UseConstant")]
    [SerializeField] private bool ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    [SerializeField] private BoolVariable Variable;
    
    public bool Value
    {
        get => UseConstant ? ConstantValue : Variable.Value;
        set => Variable.Value = value;
    }
}