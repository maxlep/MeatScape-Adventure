using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.Misc.FunctionComponents
{
    public class HoldInteraction : MonoBehaviour
    {
        [SerializeField] private TimerReference _timer;
        [SerializeField] private UnityEvent _action;
        
        public void StartTimer()
        {
            _timer.RestartTimer();
        }
        
        public void StopTimer()
        {
            _timer.StopTimer();
        }
        
        private void Update()
        {
            _timer.UpdateTime();
            if (_timer.IsFinished)
            {
                _action.Invoke();
            }
        }
    }
}