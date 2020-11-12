using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallMeatClumpAir : PlayerStateNode
{
    public override void Enter()
    {
        base.Enter();
        playerController.RecallAttempts--;
        playerController.RecallClump();
    }
}
