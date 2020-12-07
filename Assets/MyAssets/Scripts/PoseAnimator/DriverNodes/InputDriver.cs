using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class InputDriver : StateNode
    {
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Input"), Required]
        private Vector2Reference InputVector;
        
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Input"), Required]
        private Vector2Reference InputMin;
        
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Input"), Required]
        private Vector2Reference InputMax;
        
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Input"), Required]
        private FloatReference MoveSpeed;
        
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        [TabGroup("Output"), Required]
        private Vector3Reference InputVelocity;
        
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
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