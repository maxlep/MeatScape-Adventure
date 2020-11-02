using UnityEngine;
using BehaviorDesigner.Runtime;
using MyAssets.ScriptableObjects.Variables;


[System.Serializable]
public class SharedVector2Reference : SharedVariable<Vector2Reference>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedVector2Reference(Vector2Reference value) { return new SharedVector2Reference { mValue = value }; }
}