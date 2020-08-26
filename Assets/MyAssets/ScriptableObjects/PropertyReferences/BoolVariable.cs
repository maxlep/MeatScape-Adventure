using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BoolVariable", menuName = "Variables/BoolVariable", order = 0)]
public class BoolVariable : ScriptableObject
{
    [SerializeField] private bool defaultValue;
    [SerializeField] private bool runtimeValue;
    public bool Value => runtimeValue;

    private void OnEnable()
    {
        runtimeValue = defaultValue;
    }
}

[Serializable]
[InlineProperty]
public class BoolReference
{
    [HorizontalGroup("Split", LabelWidth = .09f)]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelWidth(.01f)]
    public bool UseConstant = true;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    public bool ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    public BoolVariable Variable;
    
    public bool Value => UseConstant ? ConstantValue : Variable.Value;
}