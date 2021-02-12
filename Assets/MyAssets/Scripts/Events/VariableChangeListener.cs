using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class VariableChangeListener : MonoBehaviour
{
    [InfoBox("Will subscribe increase and decrease event to OnUpdate for ALL Variables below")]
    [SerializeField] private List<BoolVariable> BoolVariables = new List<BoolVariable>();
    [SerializeField] private List<FloatVariable> FloatVariables = new List<FloatVariable>();
    [SerializeField] private List<IntVariable> IntVariables = new List<IntVariable>();
    [SerializeField] private List<Vector2Variable> Vector2Variables = new List<Vector2Variable>();
    [SerializeField] private List<Vector3Variable> Vector3Variables = new List<Vector3Variable>();
    
    public UnityEvent OnIncrease;
    public UnityEvent OnDecrease;
    
    void Start()
    {
        foreach (var boolVariable in BoolVariables)
        {
            boolVariable.Subscribe(ChangedBool);
        }
        foreach (var floatVariable in FloatVariables)
        {
            floatVariable.Subscribe(ChangedFloat);
        }
        foreach (var intVariable in IntVariables)
        {
            intVariable.Subscribe(ChangedInt);
        }
        foreach (var vector2Variable in Vector2Variables)
        {
            vector2Variable.Subscribe(ChangedVector2);
        }
        foreach (var vector3Variable in Vector3Variables)
        {
            vector3Variable.Subscribe(ChangedVector3);
        }
    }

    private void ChangedBool(bool previousValue, bool currentValue)
    {
        if (!previousValue && currentValue) OnIncrease?.Invoke();        //False -> True
        else if (previousValue && !currentValue) OnDecrease?.Invoke();   //True -> False
    }
    
    private void ChangedFloat(float previousValue, float currentValue)
    {
        if (currentValue > previousValue) OnIncrease?.Invoke();
        else if (currentValue < previousValue) OnDecrease?.Invoke();
    }
    
    private void ChangedInt(int previousValue, int currentValue)
    {
        if (currentValue > previousValue) OnIncrease?.Invoke();
        else if (currentValue < previousValue) OnDecrease?.Invoke();
    }
    
    private void ChangedVector2(Vector2 previousValue, Vector2 currentValue)
    {
        if (currentValue.sqrMagnitude > previousValue.sqrMagnitude) OnIncrease?.Invoke();
        else if (currentValue.sqrMagnitude < previousValue.sqrMagnitude) OnDecrease?.Invoke();
    }
    
    private void ChangedVector3(Vector3 previousValue, Vector3 currentValue)
    {
        if (currentValue.sqrMagnitude > previousValue.sqrMagnitude) OnIncrease?.Invoke();
        else if (currentValue.sqrMagnitude < previousValue.sqrMagnitude) OnDecrease?.Invoke();
    }
}
