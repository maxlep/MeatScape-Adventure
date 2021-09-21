using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.Misc
{
    public class AddForce : MonoBehaviour
    {
        [Header("Direction")]
        [SerializeField] private bool invertDirection;
        [SerializeField] private bool useTransform = true;
        [SerializeField] [ShowIf("useTransform")] private Transform firePoint;
        [SerializeField] [HideIf("useTransform")] private Vector3 fireDir;
        [SerializeField] private bool horizontalOnly;
        
        [Header("Force")]
        [SerializeField] private ForceMode forceMode = ForceMode.Force;
        [SerializeField] private float launchForce = 1000f;
        [SerializeField] private bool applyExplosiveForce;
        [SerializeField] private float explosiveForce = 1000f;
        [SerializeField] private float explosiveRadius = 20f;
        [SerializeField] private GameObject targetObject;
        [SerializeField] private List<Rigidbody> launchRbs = new List<Rigidbody>();
        
        [Button("Populate Rigidbodies")]
        public void PopulateRigidbodies()
        {
            launchRbs.Clear();
            Transform targetTrans = (targetObject != null) ? targetObject.transform : transform;
            GetChildRigidbodies(targetTrans);
        }
        
        private void GetChildRigidbodies(Transform targetTransform)
        {
            foreach(Transform child in targetTransform)
            {
                Rigidbody childRb = child.gameObject.GetComponent<Rigidbody>();
                if (childRb != null) launchRbs.Add(childRb);
                GetChildRigidbodies(child);
            }
        }

        public void Launch()
        {
            Vector3 dir = useTransform ? firePoint.forward : fireDir;
            if (invertDirection) dir = -dir;
            Launch(dir, transform.position);
        }
        
        private void Launch(Vector3 dir, Vector3 origin)
        {
            Vector3 forceDir = dir;
            if (horizontalOnly) forceDir = forceDir.xoz(); 
        
            foreach (var rb in launchRbs)
            {
                rb.AddForce(launchForce * forceDir, forceMode);
                if (applyExplosiveForce)
                    rb.AddExplosionForce(explosiveForce, origin, explosiveRadius);
            }
        }

        public void SetTargetObject(GameObject targetObj)
        {
            targetObject = targetObj;
        }
    }
}