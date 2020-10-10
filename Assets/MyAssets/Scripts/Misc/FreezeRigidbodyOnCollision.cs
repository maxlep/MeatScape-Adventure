using System;
using MyAssets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.Misc
{
    [RequireComponent(typeof(Rigidbody))]
    public class FreezeRigidbodyOnCollision : MonoBehaviour
    {
        // [SerializeField] private LayerMask layerMask;
        [SerializeField] private bool stickToHitObject;
        private Vector3 offsetFromObject;
        private Transform hitObject;

        [SerializeField] private UnityEvent onFreeze;
        private bool isFrozen;
        
        private Rigidbody rigidbody;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision other)
        {
            // if ((1 << other.gameObject.layer & layerMask) != 1) return;

            if (isFrozen) return;
            
            isFrozen = true;
            rigidbody.isKinematic = true;

            hitObject = other.transform;
            offsetFromObject = transform.position - hitObject.position;
            
            // var t = transform;
            // var newParent = other.transform;
            
            // var parentScale = newParent.localScale.Inverse();
            // var parentScaleProjected = new Vector3
            // {
            //     x = parentScale.ProjectComponents(t.right).magnitude,
            //     y = parentScale.ProjectComponents(t.up).magnitude,
            //     z = parentScale.ProjectComponents(t.forward).magnitude,
            // };
            // var newScale = Vector3.Scale(t.localScale, parentScaleProjected);
            // Debug.Log($"Splat {transform.localScale} {parentScale}, {parentScaleProjected}, {transform.localScale} {newScale}");
            
            // t.parent = other.transform;
            // t.SetParent(other.transform, true);
            // t.localScale = newScale;
            
            onFreeze.Invoke();
        }

        private void Update()
        {
            if (isFrozen && hitObject != null)
            {
                transform.position = hitObject.position + offsetFromObject;
            }
        }
    }
}