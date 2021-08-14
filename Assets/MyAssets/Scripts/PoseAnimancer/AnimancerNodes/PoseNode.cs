using Animancer;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class PoseNode : AnimationStateNode
    {
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private TriggerVariable animEndedTrigger;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private GameEvent animEndedEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private TriggerVariable animEventTrigger;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
        private GameEvent animEvent;
        
        [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [Required]
        private ClipState.Transition _pose;
        
        public override void Enter()
        {
            base.Enter();

            if (_pose.IsValid)
            {
                _animatable.Animancer.States.GetOrCreate(_pose);
                _startNode.AnimancerLayer.Play(_pose);
            }
            else
            {
                _startNode.AnimancerLayer.Stop();
            }
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