using UnityEngine;
using BehaviorDesigner.Runtime;

[System.Serializable]
public class SharedEnemyController : SharedVariable<EnemyController>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedEnemyController(EnemyController value) { return new SharedEnemyController { mValue = value }; }
}