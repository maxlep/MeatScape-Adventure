using Sirenix.OdinInspector;
using UnityEngine;

public class AnimatorUpdater : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [LabelText("Jump")] [SerializeField] private TriggerVariable triggerJumpAnim;
    private static readonly int Jump1 = Animator.StringToHash("Jump");

    private void Awake()
    {
        Debug.Log("SUBSCRIBED");
        triggerJumpAnim.OnUpdate += Jump;
    }

    private void Jump()
    {
        Debug.Log("JUMPED");
        animator.SetTrigger(Jump1);
    }
}
