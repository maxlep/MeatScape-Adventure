using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "CurveVariable", menuName = "Variables/CurveVariable", order = 0)]
    public class CurveVariable : Variable<AnimationCurve> {}
}