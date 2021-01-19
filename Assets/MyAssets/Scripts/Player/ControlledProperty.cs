using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[HideReferenceObjectPicker]
public class ControlledProperty {
    [SerializeField] bool useCustomUpdate = false;
    [HideIf("useCustomUpdate")][Required][SerializeField] private FloatVariable ControlledVariable;
    [ShowIf("useCustomUpdate")][Required][SerializeField] UnityEvent<float> CustomUpdate;
    [Space]
    [Required][HideReferenceObjectPicker][SerializeField] private CurveReference ControlCurve;

    public void Update(float percent) {
        if(useCustomUpdate) {
            CustomUpdate.Invoke(ControlCurve.Value.Evaluate(percent));
            return;
        }
        ControlledVariable.Value = ControlCurve.Value.Evaluate(percent);
    }
}
