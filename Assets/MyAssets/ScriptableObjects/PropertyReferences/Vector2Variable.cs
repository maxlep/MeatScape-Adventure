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

    public Vector2 Value
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
public class Vector2Reference
{
    [HorizontalGroup("Split", LabelWidth = .09f)] [Tooltip("Use Constant or VariableReference")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelWidth(.01f)]
    public bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    public Vector2 ConstantValue;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    public Vector2Variable Variable;

    public Vector2 Value => UseConstant ? ConstantValue : Variable.Value;
}
