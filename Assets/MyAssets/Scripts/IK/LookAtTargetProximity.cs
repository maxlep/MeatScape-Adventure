using MyAssets.ScriptableObjects.Variables;
using RootMotion.FinalIK;
using UnityEngine;

namespace MyAssets.Scripts.IK
{
    public class LookAtTargetProximity : MonoBehaviour
    {
        [SerializeField] private TransformSceneReference lookAtTarget;
        [SerializeField] private LookAtIK lookAtIK;

        private void Update()
        {
            lookAtIK.solver.target = lookAtTarget.Value;
            lookAtIK.solver.IKPositionWeight = 1f;
        }
    }
}