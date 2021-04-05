using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class ScriptableObjectResetManager : MonoBehaviour
{
    [SerializeField] private VariableContainer ResetContainer;

    public void ResetValues()
    {
        foreach (var variable in ResetContainer.GetFloatVariables())
        {
            variable.Reset();
        }
        foreach (var variable in ResetContainer.GetIntVariables())
        {
            variable.Reset();
        }
        foreach (var variable in ResetContainer.GetQuaternionVariables())
        {
            variable.Reset();
        }
        foreach (var variable in ResetContainer.GetVector2Variables())
        {
            variable.Reset();
        }
        foreach (var variable in ResetContainer.GetVector3Variables())
        {
            variable.Reset();
        }
        foreach (var variable in ResetContainer.GetBoolVariables())
        {
            variable.Reset();
        }
    }
}
