using MyAssets.ScriptableObjects.Variables;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MyAssets.Scripts.VariableOperators
{
    public class RemapRange : MonoBehaviour
    {
        [SerializeField] private FloatReference _input;
        [SerializeField] private bool useCurve = true;
        [ShowIf("useCurve")][SerializeField] private CurveReference _curve;
        [HideIf("useCurve")][SerializeField] private FloatReference _oldMin;
        [HideIf("useCurve")][SerializeField] private FloatReference _oldMax;

        [SerializeField] private FloatReference _output;
        [SerializeField] private FloatReference _newMin;
        [SerializeField] private FloatReference _newMax;

        private void Update()
        {
            var oldMin = useCurve ? _curve.GetMinValue() : _oldMin.Value;
            var oldMax = useCurve ? _curve.GetMaxValue() : _oldMax.Value;
            _output.Value =
                (_input.Value - oldMin)
                / (oldMax - oldMin)
                * (_newMax.Value - _newMin.Value)
                + _newMin.Value;
        }
    }
}