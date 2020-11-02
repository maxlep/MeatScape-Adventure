using UnityEngine;
using BehaviorDesigner.Runtime;


[System.Serializable]
public class SharedTimerReference : SharedVariable<TimerReference>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedTimerReference(TimerReference value) { return new SharedTimerReference { mValue = value }; }
}