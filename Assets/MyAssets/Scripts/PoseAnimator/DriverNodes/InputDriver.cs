using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class InputDriver : StateNode
    {
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Input"), Required]
        private Vector2Reference InputVector;
        
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Input"), Required]
        private Vector2Reference InputMin;
        
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Input"), Required]
        private Vector2Reference InputMax;
        
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Input"), Required]
        private FloatValueReference MoveSpeed;
        
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Output"), Required]
        private Vector3Reference InputVelocity;
        
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Output"), Required]
        private FloatReference InputSpeed;

        public override void ExecuteFixed()
        {
            base.ExecuteFixed();

            var pct = ((InputVector.Value - InputMin.Value) / (InputMax.Value - InputMin.Value)).magnitude;
            InputSpeed.Value = pct * MoveSpeed.Value;
            InputVelocity.Value = InputSpeed.Value * InputVector.Value.normalized;
        }
    }
}