using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public enum Vector2Component
{
    X,
    Y
}

[Serializable]
[InlineProperty]
public class FloatVec2Reference
{
    [SerializeField] [HideLabel]
    private Vector2Reference Source;
    
    [SerializeField] [HideLabel] [EnumToggleButtons] [PropertySpace(0f, 3f)]
    Vector2Component Component = Vector2Component.X;
    
    public float Value
    {
        get
        {
            if (Component == Vector2Component.X)
                return Source.Value.x;
            else
                return Source.Value.y;
        }
    }
}
