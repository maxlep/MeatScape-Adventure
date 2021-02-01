using MyAssets.ScriptableObjects.Variables;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

[InlineEditor]
[HideReferenceObjectPicker]
public class ControlledProperties
{
    [SerializeField] [HideReferenceObjectPicker] [Required]
    private IntReference ControlMax;
    
    [SerializeField] [HideReferenceObjectPicker] [Required] [PropertySpace(0f, 10f)]
    private IntReference ControlCurrent;
    
    [ListDrawerSettings(DraggableItems=false)][SerializeField] [TabGroup("Update")] [GUIColor(.9f, .95f, 1f)] 
    [HideReferenceObjectPicker] [Required] [PropertySpace(5f, 0f)] [LabelText("Controlled Props - Update")]
    private List<ControlledProperty> ControlledUpdateProperties;
    
    [ListDrawerSettings(DraggableItems=false)][SerializeField] [TabGroup("LateUpdate")] [GUIColor(.9f, .95f, 1f)]
    [HideReferenceObjectPicker] [Required] [LabelText("Controlled Props - LateUpdate")]
    private List<ControlledProperty> ControlledLateUpdateProperties;
    
    [ListDrawerSettings(DraggableItems=false)][SerializeField] [TabGroup("FixedUpdate")] [GUIColor(.9f, .95f, 1f)] 
    [HideReferenceObjectPicker] [Required] [LabelText("Controlled Props - FixedUpdate")]
    private List<ControlledProperty> ControlledFixedUpdateProperties;

    private List<Action> ControlledUpdatePropertiesCallbacks;
    private List<Action> ControlledLateUpdatePropertiesCallbacks;
    private List<Action> ControlledFixedUpdatePropertiesCallbacks;

    public void Setup() {
        ControlledUpdatePropertiesCallbacks = new List<Action>();
        ControlledLateUpdatePropertiesCallbacks = new List<Action>();
        ControlledFixedUpdatePropertiesCallbacks = new List<Action>();
        ControlCurrent.Subscribe(() => {
            ControlledUpdateProperties?.ForEach((property) => {
                ControlledUpdatePropertiesCallbacks.Add(() => property.Update((float)ControlCurrent.Value / ControlMax.Value));
            });
            ControlledLateUpdateProperties?.ForEach((property) => {
                ControlledLateUpdatePropertiesCallbacks.Add(() => property.Update((float)ControlCurrent.Value / ControlMax.Value));
                
            });
            ControlledFixedUpdateProperties?.ForEach((property) => {
                ControlledFixedUpdatePropertiesCallbacks.Add(() => property.Update((float)ControlCurrent.Value / ControlMax.Value));
                
            });
        });
        ControlCurrent.Value = ControlCurrent.Value;
    }

    private void UpdateProperties(List<Action> UpdatePropertiesCallbacks) {
        UpdatePropertiesCallbacks.ForEach((callback) => callback.Invoke());
        UpdatePropertiesCallbacks.Clear();
    }

    public void Update() {
        this.UpdateProperties(ControlledUpdatePropertiesCallbacks);
    }

    public void LateUpdate() {
        this.UpdateProperties(ControlledLateUpdatePropertiesCallbacks);
    }

    public void FixedUpdate() {
        this.UpdateProperties(ControlledFixedUpdatePropertiesCallbacks);
    }
}

