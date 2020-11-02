using UnityEngine;
using BehaviorDesigner.Runtime;
using MyAssets.ScriptableObjects.Variables;

[System.Serializable]
public class SharedQuaternionReference : SharedVariable<QuaternionReference>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedQuaternionReference(QuaternionReference value) { return new SharedQuaternionReference { mValue = value }; }
}