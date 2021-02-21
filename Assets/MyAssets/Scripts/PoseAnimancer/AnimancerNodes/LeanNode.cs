using Animancer.Examples.Jobs;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.PoseAnimancer.AnimancerJobs;
using MyAssets.Scripts.PoseAnimator.AnimationNodes;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerNodes
{
    public class LeanNode : AnimationStateNode
    {
        [SerializeField] private FloatValueReference _angle;
        [SerializeField] private Vector3Reference _axis;
        
        private SpecificLean _lean;
        
        public override void Enter()
        {
            base.Enter();
            
            _lean = new SpecificLean(sharedData.Animatable.Animancer.Playable);
            _angle.Subscribe(() =>
            {
                _lean.Angle = _angle.Value;
            });
            _axis.Subscribe(() =>
            {
                _lean.Axis = _axis.Value;
            });
        }

        public override void Exit()
        {
            base.Exit();
            
            _lean.Destroy();
        }
    }
}