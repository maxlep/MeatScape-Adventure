using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[InlineProperty]
public class Vector2Vec3Reference
{
    [SerializeField] [HideLabel]
    private Vector3Reference Source;
    
    [SerializeField] [HideLabel] [EnumToggleButtons] [HorizontalGroup("")]
    Vector3Component ComponentX = Vector3Component.X;
    
    [SerializeField] [HideLabel] [EnumToggleButtons][HorizontalGroup("")] [PropertySpace(0f, 3f)]
    Vector3Component ComponentY = Vector3Component.X;
    
    public Vector2 Value
    {
        get
        {
            Vector2 result;

            if (ComponentX == Vector3Component.X)
                result.x = Source.Value.x;
            else if (ComponentX == Vector3Component.Y)
                result.x = Source.Value.y;
            else
                result.x = Source.Value.z;
            
            if (ComponentY == Vector3Component.X)
                result.y = Source.Value.x;
            else if (ComponentY == Vector3Component.Y)
                result.y = Source.Value.y;
            else
                result.y = Source.Value.z;

            return result;
        }
    }
}
