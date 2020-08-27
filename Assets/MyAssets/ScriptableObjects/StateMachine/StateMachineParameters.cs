using System;
using System.Collections.Generic;
using System.Configuration;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "StateMachineParameters", menuName = "ScriptableObjects/StateMachineParameters", order = 0)]
public class StateMachineParameters : SerializedScriptableObject
{
    [Required] public List<BoolVariable> BoolParameters;
    [Required] public List<TriggerVariable> TriggerParameters;
    [Required] public List<FloatVariable> FloatParameters;
    [Required] public List<IntVariable> IntParameters;
}

