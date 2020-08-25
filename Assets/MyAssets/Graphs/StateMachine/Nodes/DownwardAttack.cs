using Sirenix.OdinInspector;
using UnityEngine;


public class DownwardAttack : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] public float throwDelay = .4f;
    [FoldoutGroup("")] [LabelWidth(120)] public float downwardForce = 1000;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }
    
    public override void Enter()
    {
        base.Enter();

        GameObject meatClump = playerController.GetMeatClump();
        Transform firePoint = playerController.GetFirePoint();
        Quaternion startRotation = Quaternion.LookRotation(firePoint.forward, Vector3.up);

        GameObject thrownClump = Instantiate(meatClump, firePoint.position, startRotation);
        Rigidbody clumpRB = thrownClump.GetComponent<Rigidbody>();
        clumpRB.AddForce(Vector3.down * downwardForce);
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
        
        LeanTween.value(0f, 1f, throwDelay)
            .setOnComplete(_ =>
            {
                parameters.SetBool("WaitedAttackDelay", true);
            });
    }
}