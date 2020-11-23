using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace MyAssets.Graphs.StateMachine.Nodes
{
    public class DispenseMeat : PlayerStateNode
    {
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Throw")] [Required]
        private GameObject Ammo;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Throw")] [Required]
        private TransformSceneReference FirePoint;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Throw")] [Required]
        private FloatReference ThrowSpeed;
        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Throw")] [Required]
        private FloatReference ThrowFalloffFactor;

        [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] [TabGroup("Input")] [Required]
        private Vector3Reference CachedVelocity;

        public override void Enter()
        {
            base.Enter();

            ThrowClump();
        }

        protected virtual void ThrowClump()
        {
            var moveDir = CachedVelocity.Value.xoz().normalized;
            if (moveDir.RoundNearZero() == Vector3.zero)
            {
                moveDir = FirePoint.Value.forward;
            }
            // var fireDir = ((Vector3.down * 0.7f) - moveDir).normalized;
            var fireDir = FirePoint.Value.forward;

            MeatClumpController clump = playerController.DetachClump(-moveDir);
            clump.transform.position = FirePoint.Value.position + Vector3.up;
            clump.SetMoving(ThrowSpeed.Value, fireDir, ThrowFalloffFactor.Value);
        }
    }
}