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
            
            _animatable.Animancer.States.GetOrCreate(_pose);
            _animatable.Animancer.Layers[0].Play(_pose);
        }

        public virtual void ActiveEndTrigger()
        {
            if (animEndedTrigger != null) animEndedTrigger.Activate();
        }
    }
}