using System.Reflection;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;


public class ForwardAttack : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private GameObject ammo;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference throwDelay;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference forwardForce;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private FloatReference upwardForce;
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private BoolReference waitedAttackDelay;

    private bool clumpThrown;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }
    
    public override void Enter()
    {
        base.Enter();

        if(playerController.CurrentSize == PlayerSize.Small) return;
        
        Transform firePoint = playerController.GetFirePoint();
        Quaternion startRotation = Quaternion.LookRotation(firePoint.forward, Vector3.up);

        GameObject thrownClump = Instantiate(ammo, firePoint.position, startRotation);
        Rigidbody clumpRB = thrownClump.GetComponent<Rigidbody>();
        clumpRB.AddForce(firePoint.forward * forwardForce.Value + Vector3.up * upwardForce.Value);

        clumpThrown = true;
        playerController.CurrentSize -= 1;
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

        if(!clumpThrown) return;

        waitedAttackDelay.Value = false;
        
        LeanTween.value(0f, 1f, throwDelay.Value)
            .setOnComplete(_ =>
            {
                waitedAttackDelay.Value = true;
            });

        clumpThrown = false;
    }
}