using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/IntVariable", order = 0)]
    public class IntVariable : Variable<int> {}
}