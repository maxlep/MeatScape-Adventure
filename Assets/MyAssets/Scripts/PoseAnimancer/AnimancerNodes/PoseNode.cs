using Animancer;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class PoseNode : AnimationStateNode
    {
        [SerializeField] private TriggerVariable animEndedTrigger;
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
        }
    }
}