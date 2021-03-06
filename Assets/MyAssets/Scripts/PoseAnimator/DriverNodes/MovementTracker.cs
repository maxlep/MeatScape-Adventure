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
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private Transform probe;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private int numFrames;
        
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private Vector3Variable outVel;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private Vector3Variable outAccel;
        // [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private FloatVariable outAccelFactor;

        private LinkedList<Vector3> posHist;
        private LinkedList<Vector3> velHist;
        
        public override void Initialize(StateMachineGraph parentGraph)
        {
            base.Initialize(parentGraph);
        }

        public override void RuntimeInitialize(int startNodeIndex)
        {
            base.RuntimeInitialize(startNodeIndex);

            probe = playerController.transform;
        }

        public override void Enter()
        {
            posHist = new LinkedList<Vector3>();
            velHist = new LinkedList<Vector3>();
            posHist.AddFirst(probe.position);
            velHist.AddFirst(Vector3.zero);
        }

        public override void Execute()
        {
            base.Execute();
            
            if (posHist.Count > numFrames) posHist.RemoveLast();
            if (velHist.Count > numFrames) velHist.RemoveLast();

            var pos = probe.position;
            var vel = (pos - posHist.First.Value) / Time.deltaTime;
            var accel = vel - velHist.First.Value;
            vel = vel.RoundNearZero();
            accel = accel.RoundNearZero();
            // var accelFactor = (1 + Vector3.Dot(accel.normalized, vel.normalized)) / 8f;
            posHist.AddFirst(pos);
            velHist.AddFirst(vel);

            outVel.Value = vel;
            outAccel.Value = accel;
            // outAccelFactor.Value = accelFactor;

            // Debug.Log($"Execute fixed {name}, {pos}, {vel}, {accel}");
            // Debug.Assert(vel.y > 0);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}