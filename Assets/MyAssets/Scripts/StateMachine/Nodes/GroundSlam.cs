using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class GroundSlam : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private bool AddOverlayed;
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] private FloatReference SlamForce;

    
    public override void Enter()
    {
        base.Enter();
        
        if (AddOverlayed)
            playerController.AddImpulse(Vector3.down * SlamForce.Value);
        else
            playerController.AddImpulseOverlayed(Vector3.down * SlamForce.Value);
    }
}
