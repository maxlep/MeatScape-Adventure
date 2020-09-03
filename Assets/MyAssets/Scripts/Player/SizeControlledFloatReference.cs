using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
[HideReferenceObjectPicker]
public class SizeControlledFloatReference {
    [SerializeField] private FloatVariable ControlledVariable;
    [Space]
    [Required] [HideReferenceObjectPicker] [SerializeField] private FloatReference Small;
    [Required] [HideReferenceObjectPicker] [SerializeField] private FloatReference Medium;
    [Required] [HideReferenceObjectPicker] [SerializeField] private FloatReference Large;

    public void UpdateValue(PlayerSize playerSize) {
        if(playerSize == PlayerSize.Small) ControlledVariable.Value = Small.Value;
        if(playerSize == PlayerSize.Medium) ControlledVariable.Value = Medium.Value;
        if(playerSize == PlayerSize.Large) ControlledVariable.Value = Large.Value;
    }
}
