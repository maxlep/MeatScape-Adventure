using System;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Vector2Condition
{
    [HideLabel, Required, SerializeField] private Vector2Reference targetParameter;

    [HideLabel] public Comparison xCompare;
    [HideLabel] public Comparison yCompare;

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

    public void Init(string transitionName)
    {
        parentTransitionName = transitionName;
    }

    public bool Evaluate()
    {
        bool xIs = Compare(xCompare, targetParameter.Value.x);
        bool yIs = Compare(xCompare, targetParameter.Value.y);
        return xIs && yIs;
    }

    public override string ToString()
    {
        return $"{targetParameter.Value.x} {xCompare.comparator} {xCompare.value} && {targetParameter.Value.y} {yCompare.comparator} {yCompare.value}";
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