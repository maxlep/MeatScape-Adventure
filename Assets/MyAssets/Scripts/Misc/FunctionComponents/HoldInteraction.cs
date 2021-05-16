using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.Misc.FunctionComponents
{
    public class HoldInteraction : MonoBehaviour
    {
        [SerializeField] private TimerReference _timer;
        [SerializeField] private UnityEvent _onHoldStart;
        [SerializeField] private UnityEvent _onHoldCancelled;
        [SerializeField] private UnityEvent _onHoldFinished;
        [SerializeField] private int _activationLimit = 0;
        [ShowInInspector, ReadOnly] private int _activationCount = 0;
        private bool CanActivate => _activationCount < _activationLimit;

        public void StartHold()
        {
            _timer.RestartTimer();
            if (CanActivate)
            {
                _onHoldStart.Invoke();
            }
        }
        
        public void StopHold()
        {
            _timer.StopTimer();
            if (CanActivate)
            {
                _onHoldCancelled.Invoke();
            }
        }

        public void ResetActivationCount()
        {
            _activationCount = 0;
        }
        
        private void Update()
        {
            _timer.UpdateTime();
            if (_timer.IsFinished && CanActivate)
            {
                _activationCount++;
                _timer.ResetTimer();
                _onHoldFinished.Invoke();
            }
        }
    }
}