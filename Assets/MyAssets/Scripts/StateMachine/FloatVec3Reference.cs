using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;
using Object = UnityEngine.Object;

[Serializable]
public enum Vector3Component
{
    X,
    Y,
    Z
}

[Serializable]
[InlineProperty]
public class FloatVec3Reference
{
    [SerializeField] [HideLabel]
    private Vector3Reference Source;
    
    [SerializeField] [HideLabel] [EnumToggleButtons] [PropertySpace(0f, 3f)]
    Vector3Component Component = Vector3Component.X;
    
    public float Value
    {
        get
        {
            if (Component == Vector3Component.X)
                return Source.Value.x;
            else if (Component == Vector3Component.Y)
                return Source.Value.y;
            else
                return Source.Value.z;
        }
    }
}
