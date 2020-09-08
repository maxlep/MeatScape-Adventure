﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class MovementTracker : PlayerStateNode
    {
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private Transform probe;
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private int numFrames;
        
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private Vector2Variable outVelHoriz;
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private Vector2Variable outAccelHoriz;
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private FloatVariable outVelVert;
        [HideIf("$zoom"), LabelWidth(120), SerializeField] private FloatVariable outAccelVert;

        private LinkedList<Vector3> posHist;
        private LinkedList<Vector3> velHist;
        
        public override void Initialize(StateMachineGraph parentGraph)
        {
            base.Initialize(parentGraph);
        }

        public override void RuntimeInitialize()
        {
            base.RuntimeInitialize();

            probe = playerController.transform;
        }

        public override void Enter()
        {
            posHist = new LinkedList<Vector3>();
            velHist = new LinkedList<Vector3>();
            posHist.AddFirst(probe.position);
            velHist.AddFirst(Vector3.zero);
        }

        public override void ExecuteFixed()
        {
            base.Execute();
            
            if (posHist.Count > numFrames) posHist.RemoveLast();
            if (velHist.Count > numFrames) velHist.RemoveLast();

            var pos = probe.position;
            var vel = (pos - posHist.First.Value) / Time.deltaTime;
            var accel = vel - velHist.First.Value;
            posHist.AddFirst(pos);
            velHist.AddFirst(vel);

            outVelHoriz.Value = vel.xz().RoundNearZero();
            outAccelHoriz.Value = accel.xz().RoundNearZero();
            outVelVert.Value = vel.y.RoundNearZero();
            outAccelVert.Value = accel.y.RoundNearZero();
            Debug.Log(Mathf.Epsilon);

            Debug.Log($"Execute fixed {name}, {pos}, {vel}, {accel}");
            // Debug.Assert(vel.y > 0);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}