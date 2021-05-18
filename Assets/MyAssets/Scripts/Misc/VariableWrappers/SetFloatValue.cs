using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

public class SetFloatValue : SerializedMonoBehaviour
{
    [SerializeField] private FloatValueReference sourceValue;
    [SerializeField] private FloatReference targetValue;

    public void SetFloat()
    {
        targetValue.Value = sourceValue.Value;
    }
}
