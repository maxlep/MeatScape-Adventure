using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "Variables/BoolVariable", order = 0)]
    public class BoolVariable : Variable<bool> {}
}