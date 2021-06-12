using System;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/IntVariable", order = 0)]
    public class IntVariable : Variable<int>, IFloatValue
    {
        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetFloat() => Value;
        public float GetValue(System.Type type) => Value;

        public void Incremenet(int increment)
        {
            Value += increment;
        }

    }
}