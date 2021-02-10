using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class TimerDriver : StateNode
    {
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private TimerReference timer;

        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatReference factor;

        public override void Execute()
        {
            base.Execute();

            factor.Value = timer.ElapsedTime / timer.Duration;
        }
    }
}