using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PropertyControllers : SerializedMonoBehaviour {

    [ListDrawerSettings(DraggableItems=false)][SerializeField] [Required] 
    List<ControlledProperties> Controllers;

    void Awake() {
        Controllers.ForEach((controller) => {
            controller.Setup();
        });
    }

    void Update() {
        Controllers.ForEach((controller) => {
            controller.Update();
        });
    }

    void LateUpdate() {
        Controllers.ForEach((controller) => {
            controller.LateUpdate();
        });
    }

    void FixedUpdate() {
        Controllers.ForEach((controller) => {
            controller.FixedUpdate();
        });
    }
}
