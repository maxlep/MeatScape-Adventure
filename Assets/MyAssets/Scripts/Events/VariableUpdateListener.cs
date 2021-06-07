using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class VariableUpdateListener : SerializedMonoBehaviour
{
    [InfoBox("Will subscribe update event to OnUpdate for ALL Variables below")]
    [SerializeField] private List<BoolVariable> BoolVariables = new List<BoolVariable>();
    [SerializeField] private List<TriggerVariable> TriggerVariables = new List<TriggerVariable>();
    [SerializeField] private List<FloatVariable> FloatVariables = new List<FloatVariable>();
    [SerializeField] private List<FloatValueReference> FloatValues = new List<FloatValueReference>();
    [SerializeField] private List<IntVariable> IntVariables = new List<IntVariable>();
    [SerializeField] private List<TimerVariable> TimerVariables = new List<TimerVariable>();
    [SerializeField] private List<Vector2Variable> Vector2Variables = new List<Vector2Variable>();
    [SerializeField] private List<Vector3Variable> Vector3Variables = new List<Vector3Variable>();

    public UnityEvent OnUpdate;
    
    private void InvokeUpdate()
    {
        OnUpdate?.Invoke();
    }
    
#region Lifecycle
    void Start()
    {
        foreach (var boolVariable in BoolVariables)
        {
            boolVariable.Subscribe(InvokeUpdate);
        }
        foreach (var triggerVariable in TriggerVariables)
        {
            triggerVariable.Subscribe(InvokeUpdate);
        }
        foreach (var floatVariable in FloatVariables)
        {
            floatVariable.Subscribe(InvokeUpdate);
        } // FloatVariable is deprecated for FloatValue, but keep here to not break existing serialization values
        foreach (var floatValue in FloatValues)
        {
            floatValue.Subscribe(InvokeUpdate);
        }
        foreach (var intVariable in IntVariables)
        {
            intVariable.Subscribe(InvokeUpdate);
        }
        foreach (var timerVariable in TimerVariables)
        {
            timerVariable.Subscribe(InvokeUpdate);
        }
        foreach (var vector2Variable in Vector2Variables)
        {
            vector2Variable.Subscribe(InvokeUpdate);
        }
        foreach (var vector3Variable in Vector3Variables)
        {
            vector3Variable.Subscribe(InvokeUpdate);
        }
    }

    private void OnDestroy()
    {
        foreach (var boolVariable in BoolVariables)
        {
            boolVariable.Unsubscribe(InvokeUpdate);
        }
        foreach (var triggerVariable in TriggerVariables)
        {
            triggerVariable.Unsubscribe(InvokeUpdate);
        }
        foreach (var floatVariable in FloatVariables)
        {
            floatVariable.Unsubscribe(InvokeUpdate);
        } // FloatVariable is deprecated for FloatValue, but keep here to not break existing serialization values
        foreach (var floatValue in FloatValues)
        {
            floatValue.Unsubscribe(InvokeUpdate);
        }
        foreach (var intVariable in IntVariables)
        {
            intVariable.Unsubscribe(InvokeUpdate);
        }
        foreach (var timerVariable in TimerVariables)
        {
            timerVariable.Unsubscribe(InvokeUpdate);
        }
        foreach (var vector2Variable in Vector2Variables)
        {
            vector2Variable.Unsubscribe(InvokeUpdate);
        }
        foreach (var vector3Variable in Vector3Variables)
        {
            vector3Variable.Unsubscribe(InvokeUpdate);
        }
    }
#endregion

}
