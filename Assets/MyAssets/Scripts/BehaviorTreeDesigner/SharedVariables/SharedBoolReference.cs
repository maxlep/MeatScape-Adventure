using UnityEngine;
using BehaviorDesigner.Runtime;
using MyAssets.ScriptableObjects.Variables;

[System.Serializable]
public class SharedBoolReference : SharedVariable<BoolReference>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedBoolReference(BoolReference value) { return new SharedBoolReference { mValue = value }; }
}