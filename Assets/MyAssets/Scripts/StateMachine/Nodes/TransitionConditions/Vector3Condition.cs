using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Vector3Condition : ITransitionCondition
{
    [HideLabel, Required, SerializeField] private Vector3Reference targetParameter;

    [HideLabel] public Comparison xCompare;
    [HideLabel] public Comparison yCompare;
    [HideLabel] public Comparison zCompare;

    private string parentTransitionName = "";

    
    public enum Comparator
    {
        Ignore,
        LessThan,
        GreaterThan,
        AbsGreaterThan,
        AbsLessThan,
    }
    
    [System.Serializable]
    public struct Comparison
    {
        public Comparator comparator;
        public float value;
    }

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate(TriggerVariable receivedTrigger)
    {
        bool xIs = Compare(xCompare, targetParameter.Value.x);
        bool yIs = Compare(yCompare, targetParameter.Value.y);
        bool zIs = Compare(zCompare, targetParameter.Value.z);
        return xIs && yIs && zIs;
    }

    public override string ToString()
    {
        return $"{targetParameter.Value.x} {xCompare.comparator} {xCompare.value} && {targetParameter.Value.y} {yCompare.comparator} {yCompare.value} && {targetParameter.Value.z} {zCompare.comparator} {zCompare.value}";
    }

    private bool Compare(Comparison comparison, float paramValue)
    {
        switch (comparison.comparator)
        {
            case Comparator.GreaterThan:
                return paramValue > comparison.value;
            case Comparator.LessThan:
                return paramValue < comparison.value;
            case Comparator.AbsGreaterThan:
                return Mathf.Abs(paramValue) > comparison.value;
            case Comparator.AbsLessThan:
                return Mathf.Abs(paramValue) < comparison.value;
            case Comparator.Ignore:
                return true;
        }

        return false;
    }
}