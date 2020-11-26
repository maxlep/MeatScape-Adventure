using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    [CreateAssetMenu(fileName = "Vector3Variable", menuName = "Variables/Vector3Variable", order = 0)]
    public class Vector3Variable : Variable<Vector3> {}
}