using UnityEngine;
using BehaviorDesigner.Runtime;
using MyAssets.ScriptableObjects.Variables;

[System.Serializable]
public class SharedVector3Reference : SharedVariable<Vector3Reference>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedVector3Reference(Vector3Reference value) { return new SharedVector3Reference { mValue = value }; }
}