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
        [SerializeField] private TriggerVariable animEndedTrigger;
        [SerializeField] private GameEvent animEndedEvent;
        [SerializeField] private TriggerVariable animEventTrigger;
        [SerializeField] private GameEvent animEvent;
        [SerializeField] private ClipState.Transition _pose;

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