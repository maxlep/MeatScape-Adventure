using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "Variables/FloatVariable", order = 0)]
    public class FloatVariable : Variable<float> {}
}