using System.Collections;
using System.Collections.Generic;
using Animancer;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class Mixer1DNode : AnimationStateNode
    {
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private TriggerVariable animEndedTrigger;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private GameEvent animEndedEvent;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private TriggerVariable animEventTrigger;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private GameEvent animEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private FloatValueReference controlParameter;

        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        private LinearMixerState.Transition _Mixer;

        public override void Enter()
        {
            base.Enter();

            _animatable.Animancer.States.GetOrCreate(_Mixer);
            _startNode.AnimancerLayer.Play(_Mixer);
        }

        public override void Execute()
        {
            base.Execute();
            _Mixer.State.Parameter = controlParameter.Value;
        }

        public virtual void ActiveEndTrigger()
        {
            if (animEndedTrigger != null) animEndedTrigger.Activate();
            if (animEndedEvent != null) animEndedEvent.Raise();
        }

        public virtual void ActiveEventTrigger()
        {
            if (animEventTrigger != null) animEventTrigger.Activate();
            if (animEvent != null) animEvent.Raise();
        }
    }
}
