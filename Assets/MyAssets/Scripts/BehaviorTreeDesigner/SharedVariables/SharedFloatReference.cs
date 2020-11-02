using UnityEngine;
using BehaviorDesigner.Runtime;
using MyAssets.ScriptableObjects.Variables;

[System.Serializable]
public class SharedFloatReference : SharedVariable<FloatReference>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedFloatReference(FloatReference value) { return new SharedFloatReference { mValue = value }; }
}