using Sirenix.OdinInspector;
using UnityEngine;


public class DownwardAttack : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] [SerializeField] private GameObject ammo;
    [FoldoutGroup("")] [LabelWidth(120)] [SerializeField] private FloatReference throwDelay;
    [FoldoutGroup("")] [LabelWidth(120)] [SerializeField] private FloatReference downwardForce;
    [FoldoutGroup("")] [LabelWidth(120)] [SerializeField] private BoolReference waitedAttackDelay;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }
    
    public override void Enter()
    {
        base.Enter();

        Transform firePoint = playerController.GetFirePoint();
        Quaternion startRotation = Quaternion.LookRotation(firePoint.forward, Vector3.up);

        GameObject thrownClump = Instantiate(ammo, firePoint.position, startRotation);
        Rigidbody clumpRB = thrownClump.GetComponent<Rigidbody>();
        clumpRB.AddForce(Vector3.down * downwardForce.Value);
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