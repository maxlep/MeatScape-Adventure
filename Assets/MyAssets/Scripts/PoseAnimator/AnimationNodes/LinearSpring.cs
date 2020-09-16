﻿using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class LinearSpring : PlayerStateNode
    {
        [HideIf("$zoom"), LabelWidth(120), SerializeField] [EnumToggleButtons] protected Vector3Axes axis;

        [HideIf("$zoom"), LabelWidth(120), SerializeField] protected FloatVec3Reference inputVelocity;
    
        [HideIf("$zoom"), LabelWidth(120), SerializeField] protected FloatReference outWeight;

        [HideIf("$zoom"), LabelWidth(120), SerializeField] protected float min, max;

        private float startVelocity;
        [SerializeField] float peakThreshold = 20f;

        public override void Initialize(StateMachineGraph parentGraph)
        {
            base.Initialize(parentGraph);
        }

        public override void Execute()
        {
            var raw = inputVelocity.Value;
            var divisor = max;
            var pct = raw / divisor;
            if (raw > 0) pct = 1 - pct;
        
            // Debug.Log($"{name} {raw}, {divisor}, {pct}");
            outWeight.Value = pct;
        }

        public override void Enter()
        {
            base.Enter();
            startVelocity = inputVelocity.Value;
        }

        public override void Exit()
        {
            base.Exit();
            startVelocity = 0;
        }
    }
}
