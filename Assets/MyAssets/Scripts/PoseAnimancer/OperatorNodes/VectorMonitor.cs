using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimancer.OperatorNodes
{
    public class VectorMonitor : StateNode
    {
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private bool SetOnEnter;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField] private bool SetOnUpdate;
        
        [HideIf("$collapsed"),LabelWidth(LABEL_WIDTH),TitleGroup("Input"),SerializeField] private Vector3Reference _vector;
        [HideIf("$collapsed"),LabelWidth(LABEL_WIDTH),TitleGroup("Input"),SerializeField] private FloatValueReference _framesOfHistory;
        [HideIf("$collapsed"),LabelWidth(LABEL_WIDTH),TitleGroup("Output"),SerializeField] private FloatReference _magnitudeOut;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), TitleGroup("Output"), SerializeField] private Vector3Reference _averageDelta;

        private LinkedList<Vector3> _previousValues;
        
        private void UpdateValue()
        {
            _magnitudeOut.Value = _vector.Value.sqrMagnitude;
        }

        public override void Enter()
        {
            base.Enter();
            
            _previousValues = new LinkedList<Vector3>();
            _previousValues.AddFirst(_vector.Value);

            if (SetOnEnter)
            {
                UpdateValue();
            }

            if (SetOnUpdate)
            {
                _vector.Subscribe(UpdateValue);
            }
        }

        public override void Exit()
        {
            base.Exit();

            _vector.Unsubscribe(UpdateValue);
        }

        public override void ExecuteFixed()
        {
            base.ExecuteFixed();

            while (_previousValues.Count >= _framesOfHistory.Value)
            {
                _previousValues.RemoveLast();
            }
            _previousValues.AddFirst(_vector.Value);
            
            Vector3 sum = Vector3.zero;
            foreach (var previousValue in _previousValues)
            {
                sum.x += previousValue.x;
                sum.y += previousValue.y;
                sum.z += previousValue.z;
            }
            _averageDelta.Value = sum / _framesOfHistory.Value;
        }
    }
}