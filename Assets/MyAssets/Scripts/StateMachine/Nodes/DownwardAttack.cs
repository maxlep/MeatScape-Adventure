﻿using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;


public class DownwardAttack : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private GameObject ammo;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference throwDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference throwSpeed;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private BoolReference waitedAttackDelay;
    [HideIf("$zoom")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private TransformSceneReference firePoint;
    
    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }
    
    public override void Enter()
    {
        base.Enter();

        Quaternion startRotation = Quaternion.LookRotation(-firePoint.Value.up, firePoint.Value.forward);

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