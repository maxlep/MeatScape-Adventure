using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.DriverNodes
{
    public class VectorSqrMagnitude : StateNode
    {
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private bool SetOnEnter;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private bool SetOnUpdate;
        
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private Vector3Reference Vector;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatReference MagnitudeOut;

        private void UpdateValue()
        {
            MagnitudeOut.Value = Vector.Value.sqrMagnitude;
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
                Vector.Subscribe(UpdateValue);
            }
        }

        public override void Exit()
        {
            base.Exit();

            Vector.Unsubscribe(UpdateValue);
        }
    }
}