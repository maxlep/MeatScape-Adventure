using MyAssets.ScriptableObjects.Variables;
using RootMotion.FinalIK;
using UnityEngine;

namespace MyAssets.Scripts.IK
{
    public class LookAtTargetProximity : MonoBehaviour
    {
        [SerializeField] private TransformSceneReference lookAtTarget;
        [SerializeField] private LookAtIK lookAtIK;
        [SerializeField] private bool runOnStart = true;

        private void Start()
        {
            if (runOnStart) SetTarget();
        }

        public void SetTarget()
        {
            lookAtIK.solver.target = lookAtTarget.Value;
            lookAtIK.solver.IKPositionWeight = 1f;
        }
    }
}