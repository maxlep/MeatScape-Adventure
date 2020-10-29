using UnityEngine;
using BehaviorDesigner.Runtime;
using UnityEngine.AI;

[System.Serializable]
public class SharedNavMeshAgent : SharedVariable<NavMeshAgent>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedNavMeshAgent(NavMeshAgent value) { return new SharedNavMeshAgent { mValue = value }; }
}