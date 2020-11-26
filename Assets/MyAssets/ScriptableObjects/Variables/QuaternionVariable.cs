using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "QuaternionVariable", menuName = "Variables/QuaternionVariable", order = 0)]
    public class QuaternionVariable : Variable<Quaternion> {}
}