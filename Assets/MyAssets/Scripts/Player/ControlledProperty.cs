using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[HideReferenceObjectPicker]
public class ControlledProperty {
    
    [Title("")]
    [SerializeField] bool useCustomUpdate = false;
    
    [Required][HideReferenceObjectPicker][SerializeField] private CurveReference ControlCurve;

    [HideIf("useCustomUpdate")][Required][SerializeField] [HideReferenceObjectPicker] 
    private FloatVariable ControlledVariable;

    [ShowIf("useCustomUpdate")][Required][SerializeField] [HideReferenceObjectPicker] 
    [PropertySpace(5f, 0f)]
    UnityEvent<float> CustomUpdate;

    public void Update(float percent) {
        if(useCustomUpdate) {
            CustomUpdate.Invoke(ControlCurve.Value.Evaluate(percent));
            return;
        }
        ControlledVariable.Value = ControlCurve.Value.Evaluate(percent);
    }
}
