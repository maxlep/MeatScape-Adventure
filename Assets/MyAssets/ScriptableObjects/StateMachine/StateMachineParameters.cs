using System;
using System.Collections.Generic;
using System.Configuration;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "StateMachineParameters", menuName = "ScriptableObjects/StateMachineParameters", order = 0)]
public class StateMachineParameters : SerializedScriptableObject
{
    [DictionaryDrawerSettings(KeyLabel = "Name")]
    public Dictionary<string, BoolValue> BoolParameters;
    
    [DictionaryDrawerSettings(KeyLabel = "Name", ValueLabel = "")]
    public Dictionary<string, TriggerValue> TriggerParameters;
    
    [DictionaryDrawerSettings(KeyLabel = "Name")]
    public Dictionary<string, FloatValue> FloatParameters;
    
    [DictionaryDrawerSettings(KeyLabel = "Name")]
    public Dictionary<string, IntValue> IntParameters;

    public void ResetParametersToDefault()
    {
        foreach (var key in BoolParameters.Keys)
            BoolParameters[key].SetValue(BoolParameters[key].DefaultValue);

        foreach (var key in TriggerParameters.Keys)
            TriggerParameters[key].SetValue(false);

        foreach (var key in FloatParameters.Keys)
            FloatParameters[key].SetValue(FloatParameters[key].DefaultValue);

        foreach (var key in IntParameters.Keys)
            IntParameters[key].SetValue(IntParameters[key].DefaultValue);
    }
    
    public bool GetBool(string paramName)
    {
        if (BoolParameters.ContainsKey(paramName))
            return BoolParameters[paramName].Value;
        else
            Debug.LogError($"Can't find Bool parameter {paramName}!");
        
        return false;
    }
    
    public void SetBool(string paramName, bool value)
    {
        if (BoolParameters.ContainsKey(paramName))
            BoolParameters[paramName].SetValue(value);
        else
            Debug.LogError($"Can't find Bool parameter {paramName}!");
    }
    
    public bool GetTrigger(string paramName)
    {
        if (TriggerParameters.ContainsKey(paramName))
            return TriggerParameters[paramName].Value;
        else
            Debug.LogError($"Can't find Trigger parameter {paramName}!");
        
        return false;
    }
    
    public void SetTrigger(string paramName, bool value)
    {
        if (TriggerParameters.ContainsKey(paramName))
            TriggerParameters[paramName].SetValue(value);
        else
            Debug.LogError($"Can't find Trigger parameter {paramName}!");
    }
    
    public float GetFloat(string paramName)
    {
        if (FloatParameters.ContainsKey(paramName))
            return FloatParameters[paramName].Value;
        else
            Debug.LogError($"Can't find Float parameter {paramName}!");
        
        return 0f;
    }
    
    public void SetFloat(string paramName, float value)
    {
        if (FloatParameters.ContainsKey(paramName))
            FloatParameters[paramName].SetValue(value);
        else
            Debug.LogError($"Can't find Float parameter {paramName}!");
    }
    
    public int GetInt(string paramName)
    {
        if (IntParameters.ContainsKey(paramName))
            return IntParameters[paramName].Value;
        else
            Debug.LogError($"Can't find Int parameter {paramName}!");
        
        return -1;
    }
    
    public void SetInt(string paramName, int value)
    {
        if (IntParameters.ContainsKey(paramName))
            IntParameters[paramName].SetValue(value);
        else
            Debug.LogError($"Can't find Int parameter {paramName}!");
    }
}

[InlineProperty(LabelWidth = 90)]
[HideReferenceObjectPicker]
public class BoolValue
{
    public bool Value;
    [LabelText("Default")] public bool DefaultValue;

    public void SetValue(bool value) => Value = value;
}

[InlineProperty(LabelWidth = 90)]
[HideReferenceObjectPicker]
public class TriggerValue
{
    [HideInInspector] public bool Value;
    
    public void SetValue(bool value) => Value = value;
}

[InlineProperty(LabelWidth = 90)]
[HideReferenceObjectPicker]
public class FloatValue
{
    public float Value;
    [LabelText("Default")] public float DefaultValue;
    
    public void SetValue(float value) => Value = value;
}

[InlineProperty(LabelWidth = 90)]
[HideReferenceObjectPicker]
public class IntValue
{
    public int Value;
    [LabelText("Default")] public int DefaultValue;
    
    public void SetValue(int value) => Value = value;
}

