using UnityEngine;
using BehaviorDesigner.Runtime;
using MyAssets.ScriptableObjects.Variables;

[System.Serializable]
public class SharedIntReference : SharedVariable<IntReference>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedIntReference(IntReference value) { return new SharedIntReference { mValue = value }; }
}