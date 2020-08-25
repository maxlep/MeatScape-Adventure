using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "StateMachineParameters", menuName = "ScriptableObjects/StateMachineParameters", order = 0)]
public class StateMachineParameters : ScriptableObject
{
    public BoolParameter[] BoolParameters;
    public TriggerParameter[] TriggerParameters;
    public FloatParameter[] FloatParameters;
    public IntParameter[] IntParameters;

    
}

[System.Serializable]
public class BoolParameter
{
    public string name;
    public bool value;
}

[System.Serializable]
public class TriggerParameter
{
    public string name;
    public bool value;
}

[System.Serializable]
public class FloatParameter
{
    public string name;
    public float value;
}

[System.Serializable]
public class IntParameter
{
    public string name;
    public int value;
}
