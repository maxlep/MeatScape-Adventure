using UnityEngine.InputSystem;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.Scripts.Utils;

public class ForwardAttack : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private GameObject ammo;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference throwDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference throwSpeed;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private BoolReference waitedAttackDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TransformSceneReference firePoint;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TransformSceneReference PlayerCameraTransform;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private Vector2Reference MoveInput;

    private bool clumpThrown;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    private Vector3 GetMoveDirection()
    {
        Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        return new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }
    
    public override void Enter()
    {
        base.Enter();

        Vector3 fireDirection = GetMoveDirection().normalized;
        if(Mathf.Approximately(fireDirection.magnitude, 0)) fireDirection = firePoint.Value.forward;
        
        Quaternion startRotation = Quaternion.LookRotation(fireDirection, Vector3.up);

        GameObject thrownClump = Instantiate(ammo, firePoint.Value.position, startRotation);
        MeatClumpController clumpScript = thrownClump.GetComponent<MeatClumpController>();
        clumpScript.Initialize(playerController, throwSpeed.Value);
        
        if(!playerController.unlimitedClumps) playerController.CurrentSize -= 1;
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void ExecuteFixed()
    {
        base.ExecuteFixed();
    }

    public override void Exit()
    {
        base.Exit();

        waitedAttackDelay.Value = false;
        
        LeanTween.value(0f, 1f, throwDelay.Value)
            .setOnComplete(_ =>
            {
                waitedAttackDelay.Value = true;
            });
    }
}