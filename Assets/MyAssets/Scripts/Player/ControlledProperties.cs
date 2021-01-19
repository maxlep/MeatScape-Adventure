using MyAssets.ScriptableObjects.Variables;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

[InlineEditor]
public class ControlledProperties
{
    [SerializeField] private IntReference ControlMax;
    [SerializeField] private IntReference ControlCurrent;
    [ListDrawerSettings(DraggableItems=false)][SerializeField] private List<ControlledProperty> ControlledUpdateProperties;
    [ListDrawerSettings(DraggableItems=false)][SerializeField] private List<ControlledProperty> ControlledLateUpdateProperties;
    [ListDrawerSettings(DraggableItems=false)][SerializeField] private List<ControlledProperty> ControlledFixedUpdateProperties;

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

