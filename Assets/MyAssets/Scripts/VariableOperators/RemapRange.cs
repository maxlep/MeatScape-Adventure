using System;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MyAssets.Scripts.VariableOperators
{
    public class RemapRange : SerializedMonoBehaviour
    {
        [SerializeField] private FloatValueReference _input;
        [SerializeField] private bool useCurve = true;
        [ShowIf("useCurve")][SerializeField] private CurveReference _curve;
        [HideIf("useCurve")][SerializeField] private FloatValueReference _oldMin;
        [HideIf("useCurve")][SerializeField] private FloatValueReference _oldMax;

        [SerializeField] private FloatVariable _output;
        [SerializeField] private FloatValueReference _newMin;
        [SerializeField] private FloatValueReference _newMax;

        private void Awake()
        {
            _input.Subscribe(Recalculate);
            _curve.Subscribe(Recalculate);
            _oldMin.Subscribe(Recalculate);
            _oldMax.Subscribe(Recalculate);
            _newMin.Subscribe(Recalculate);
            _newMax.Subscribe(Recalculate);
        }

        private void Recalculate()
        {
            var oldMin = useCurve ? _curve.MinValue : _oldMin.Value;
            var oldMax = useCurve ? _curve.MaxValue : _oldMax.Value;
            _output.Value =
                (_input.Value - oldMin)
                / (oldMax - oldMin)
                * (_newMax.Value - _newMin.Value)
                + _newMin.Value;
        }
    }
}