using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "Vector2Variable", menuName = "Variables/Vector2Variable", order = 0)]
    public class Vector2Variable : Variable<Vector2> {}
}