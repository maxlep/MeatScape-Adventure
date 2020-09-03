using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Vector3Condition
{
    // [ValueDropdown("GetVector3s")] [Required]
    // [HideLabel] public string TargetParameterName;
    [HideLabel, Required, SerializeField] private Vector3Reference variable;

    [HideLabel] public Comparison xCompare;
    [HideLabel] public Comparison yCompare;
    [HideLabel] public Comparison zCompare;
    
    [HideInInspector] public VariableContainer parameters;
    [HideInInspector] public Dictionary<string, Vector3Variable> parameterDict = new Dictionary<string, Vector3Variable>();

    private string parentTransitionName = "";

    
    public enum Comparator
    {
        Ignore,
        LessThan,
        GreaterThan
    }
    
    [Serializable]
    public struct Comparison
    {
        public Comparator comparator;
        public float value;
    }
    
    
    // private List<string> GetVector3s()
    // {
    //     return (parameterDict.Count > 0) ?  parameterDict.Keys.ToList() : new List<string>() {""};
    // }
    
    public void Init(VariableContainer machineParameters, string transitionName)
    {
        parameters = machineParameters;
        parentTransitionName = transitionName;
        parameterDict.Clear();
        foreach (var vector3Param in parameters.GetVector3Variables())
        {
            parameterDict.Add(vector3Param.name, vector3Param);
        }
    }

    public bool Evaluate()
    {
        // if (!parameterDict.ContainsKey(TargetParameterName))
        //     Debug.LogError($"Transition {parentTransitionName} Float Condition can't find targetParam " + 
        //                    $"{TargetParameterName}! Did the name of SO parameter change but not update in dropdown?");
        //
        // Vector3 paramValue = parameterDict[TargetParameterName].Value;

        // bool xIs = Compare(xCompare, paramValue.x);
        // bool yIs = Compare(xCompare, paramValue.y);
        // bool zIs = Compare(xCompare, paramValue.z);
        // return xIs && yIs && zIs;
        
        bool xIs = Compare(xCompare, variable.Value.x);
        bool yIs = Compare(xCompare, variable.Value.y);
        bool zIs = Compare(xCompare, variable.Value.z);
        return xIs && yIs && zIs;
    }

    public override string ToString()
    {
        // return $"{TargetParameterName} {xCompare.comparator} {xCompare.value} {yCompare.comparator} {yCompare.value} {zCompare.comparator} {zCompare.value}";
        return $"{variable.Value} {xCompare.comparator} {xCompare.value} {yCompare.comparator} {yCompare.value} {zCompare.comparator} {zCompare.value}";
    }

    private bool Compare(Comparison comparison, float paramValue)
    {
        switch (comparison.comparator)
        {
            case Comparator.GreaterThan:
                return paramValue > comparison.value;
            case Comparator.LessThan:
                return paramValue < comparison.value;
            case Comparator.Ignore:
                return true;
        }

        return false;
    }
}