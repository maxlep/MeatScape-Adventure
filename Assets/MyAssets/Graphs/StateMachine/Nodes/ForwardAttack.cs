using Sirenix.OdinInspector;
using UnityEngine;


public class ForwardAttack : PlayerStateNode
{
    [FoldoutGroup("")] [LabelWidth(120)] public float throwDelay = .4f;
    [FoldoutGroup("")] [LabelWidth(120)] public float forwardForce = 1000;
    [FoldoutGroup("")] [LabelWidth(120)] public float upwardForce = 100;

    private float lastAttackTime = Mathf.NegativeInfinity;
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
        lastAttackTime = Mathf.NegativeInfinity;
    }
    
    public override void Enter()
    {
        base.Enter();

        if (lastAttackTime + throwDelay > Time.time) 
            return;
        
        GameObject meatClump = playerController.GetMeatClump();
        Transform firePoint = playerController.GetFirePoint();
        Quaternion startRotation = Quaternion.LookRotation(firePoint.forward, Vector3.up);

        GameObject thrownClump = Instantiate(meatClump, firePoint.position, startRotation);
        Rigidbody clumpRB = thrownClump.GetComponent<Rigidbody>();
        clumpRB.AddForce(firePoint.forward * forwardForce + Vector3.up * upwardForce);
        lastAttackTime = Time.time;
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
    }
}