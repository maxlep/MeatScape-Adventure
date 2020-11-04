using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Required] [InlineEditor]
[CreateAssetMenu(fileName = "EnemyTypeContainer", menuName = "EnemyTypeContainer", order = 0)]
public class EnemyTypeContainer : ScriptableObject
{
    [AssetsOnly] [ListDrawerSettings(Expanded = true)] public List<GameObject> EnemyPrefabs;
}
