using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "StateMachineParameters", menuName = "ScriptableObjects/StateMachineParameters", order = 0)]
public class StateMachineParameters : SerializedScriptableObject
{
    public Dictionary<string, bool> BoolParameters;
    public Dictionary<string, bool> TriggerParameters;
    public Dictionary<string, float> FloatParameters;
    public Dictionary<string, int> IntParameters;

    public void SetBool(string paramName, bool value)
    {
        if (BoolParameters.ContainsKey(paramName))
            BoolParameters[paramName] = value;
        else
            Debug.Log($"Can't find Bool parameter {paramName}!");
    }
    
    public void SetTrigger(string paramName)
    {
        if (TriggerParameters.ContainsKey(paramName))
            TriggerParameters[paramName] = true;
        else
            Debug.Log($"Can't find Trigger parameter {paramName}!");
    }
    
    public void SetFloat(string paramName, float value)
    {
        if (FloatParameters.ContainsKey(paramName))
            FloatParameters[paramName] = value;
        else
            Debug.Log($"Can't find Float parameter {paramName}!");
    }
    
    public void SetInt(string paramName, int value)
    {
        if (IntParameters.ContainsKey(paramName))
            IntParameters[paramName] = value;
        else
            Debug.Log($"Can't find Int parameter {paramName}!");
    }
    
}

