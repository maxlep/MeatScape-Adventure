using Animancer;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class PoseNode : AnimationStateNode
    {
        [SerializeField] private ClipState.Transition _pose;

        public override void Enter()
        {
            base.Enter();
            
            _animatable.Animancer.States.GetOrCreate(_pose);
            _animatable.Animancer.Layers[0].Play(_pose);
        }
    }
}