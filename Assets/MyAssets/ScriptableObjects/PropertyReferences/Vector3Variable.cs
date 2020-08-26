using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Vector3Variable", menuName = "Variables/Vector3Variable", order = 0)]
public class Vector3Variable : ScriptableObject
{
    [SerializeField] private Vector3 defaultValue;
    [SerializeField] private Vector3 runtimeValue;
    public Vector3 Value => runtimeValue;

    private void OnEnable()
    {
        runtimeValue = defaultValue;
    }}

[Serializable]
[InlineProperty]
public class Vector3Reference
{
    [HorizontalGroup("Split", LabelWidth = .09f)]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelWidth(.01f)]
    public bool UseConstant = true;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    public Vector3 ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    public Vector3Variable Variable;

    public Vector3 Value => UseConstant? ConstantValue : Variable.Value;

    public enum ValueType
    {
        CONSTANT,
        VARIABLE
    }
}
