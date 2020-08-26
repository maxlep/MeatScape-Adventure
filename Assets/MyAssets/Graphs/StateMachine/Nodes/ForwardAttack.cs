using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;


public class ForwardAttack : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] public GameObject ammo;
    [FoldoutGroup("")] [LabelWidth(120)] public FloatReference throwDelay;
    [FoldoutGroup("")] [LabelWidth(120)] public FloatReference forwardForce;
    [FoldoutGroup("")] [LabelWidth(120)] public FloatReference upwardForce;

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
        clumpRB.AddForce(firePoint.forward * forwardForce.Value + Vector3.up * upwardForce.Value);
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
        parameters.SetBool("WaitedAttackDelay", false);
        
        LeanTween.value(0f, 1f, throwDelay.Value)
            .setOnComplete(_ =>
            {
                parameters.SetBool("WaitedAttackDelay", true);
            });
    }
}