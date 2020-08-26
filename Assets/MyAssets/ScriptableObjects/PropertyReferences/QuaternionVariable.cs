using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuaternionVariable", menuName = "ScriptableObjects/QuaternionVariable", order = 0)]
public class QuaternionVariable : ScriptableObject
{
    public Quaternion Value;
}

[Serializable]
public class QuaternionReference
{
    public bool UseConstant = true;
    public Quaternion ConstantValue;
    public QuaternionVariable Variable;

    public Quaternion Value => UseConstant ? ConstantValue : Variable.Value;
}