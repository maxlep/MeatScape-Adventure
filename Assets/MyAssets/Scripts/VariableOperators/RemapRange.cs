using System;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace MyAssets.Scripts.VariableOperators
{
    public class RemapRange : MonoBehaviour
    {
        [SerializeField] private FloatReference _input;
        [SerializeField] private FloatReference _oldMin;
        [SerializeField] private FloatReference _oldMax;

        [SerializeField] private FloatReference _output;
        [SerializeField] private FloatReference _newMin;
        [SerializeField] private FloatReference _newMax;

        private void Update()
        {
            _output.Value =
                (_input.Value - _oldMin.Value)
                / (_oldMax.Value - _oldMin.Value)
                * (_newMax.Value - _newMin.Value)
                + _newMin.Value;
        }
    }
}