using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class SetIntValue : MonoBehaviour
{
    [SerializeField] private IntReference sourceValue;
    [SerializeField] private IntReference targetValue;

    public void SetInt()
    {
        targetValue.Value = sourceValue.Value;
    }
}
