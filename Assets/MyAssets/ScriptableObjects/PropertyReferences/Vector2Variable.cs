using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vector2Variable", menuName = "ScriptableObjects/Vector2Variable", order = 0)]
public class Vector2Variable : ScriptableObject
{
    public Vector2 Value;
}

[Serializable]
public class Vector2Reference
{
    public bool UseConstant = true;
    public Vector2 ConstantValue;
    public Vector2Variable Variable;

    public Vector2 Value => UseConstant ? ConstantValue : Variable.Value;
}
