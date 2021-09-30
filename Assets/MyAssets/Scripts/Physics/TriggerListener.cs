using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.Misc
{
    public class TriggerListener : MonoBehaviour
    {
        [SerializeField] private LayerMask mask;
        
        public UnityEvent OnEnter;
        public UnityEvent OnStay;
        public UnityEvent OnExit;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.IsInLayerMask(mask))
                OnEnter.Invoke();
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.IsInLayerMask(mask))
                OnStay.Invoke();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.IsInLayerMask(mask))
                OnExit.Invoke();
        }
    }
}