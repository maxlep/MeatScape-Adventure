using Sirenix.OdinInspector;
using UnityEngine;

public class AnimatorUpdater : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [LabelText("Jump")] [SerializeField] private TriggerVariable triggerJumpAnim;
    private static readonly int Jump1 = Animator.StringToHash("Jump");

    private void Awake()
    {
        triggerJumpAnim.OnUpdate += Jump;
    }

    private void Jump()
    {
        animator.SetTrigger(Jump1);
    }
}
