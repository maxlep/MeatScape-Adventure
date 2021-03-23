using System;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables.ValueReferences
{
    public interface IVector3Value : IValue<Vector3>
    {
        Vector3 GetVector3();
    }
}