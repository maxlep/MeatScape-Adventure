using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.DriverNodes
{
    public class SetBool : StateNode
    {
        
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private bool SetOnEnter;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private bool SetOnUpdate;
        
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private BoolReference Value;
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private BoolReference Target;

        private void UpdateValue()
        {
            Target.Value = Value.Value;
        }
            
        public override void Enter()
        {
            base.Enter();

            if (SetOnEnter)
            {
                UpdateValue();
            }

            if (SetOnUpdate)
            {
                Value.Subscribe(UpdateValue);
            }
        }

        public override void Exit()
        {
            base.Exit();

            Value.Unsubscribe(UpdateValue);
        }
    }
}