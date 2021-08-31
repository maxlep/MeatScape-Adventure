using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[HideReferenceObjectPicker]
public class ControlledProperty
{

    [Title("")]
    [SerializeField] bool useCustomUpdate = false;
    [SerializeField] bool stopAtMax = true;

    [Required] [HideReferenceObjectPicker] [SerializeField] private CurveReference ControlCurve;

    [HideIf("useCustomUpdate")]
    [Required]
    [SerializeField]
    [HideReferenceObjectPicker]
    private FloatVariable ControlledVariable;

    [ShowIf("useCustomUpdate")]
    [Required]
    [SerializeField]
    [HideReferenceObjectPicker]
    [PropertySpace(5f, 0f)]
    UnityEvent<float> CustomUpdate;


    public void Update(float percent)
    {
        float newValue = ControlCurve.Value.Evaluate(percent);
        if(!stopAtMax && percent > 1)
        {
            newValue *= percent;
        }
        if(useCustomUpdate)
        {
            CustomUpdate.Invoke(newValue);
            return;
        }
        ControlledVariable.Value = newValue;
    }
}
