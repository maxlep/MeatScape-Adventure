﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "FloatVariable", menuName = "Variables/FloatVariable", order = 0)]
public class FloatVariable : ScriptableObject
{
    [SerializeField] private float defaultValue;
    [SerializeField] private float runtimeValue;
    public float Value => runtimeValue;

    private void OnEnable()
    {
        runtimeValue = defaultValue;
    }
}

[Serializable]
[InlineProperty]
public class FloatReference
{
    [HorizontalGroup("Split", LabelWidth = .09f)]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelWidth(.01f)]
    public bool UseConstant = true;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    public float ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    public FloatVariable Variable;

    public float Value => UseConstant ? ConstantValue : Variable.Value;
}