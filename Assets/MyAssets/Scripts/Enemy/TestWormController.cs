using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

public class TestWormController : EnemyController
{
    [SerializeField] private Transform ikTarget;
    [SerializeField] private FABRIK ikSolver;
    [SerializeField] private FollowIK followIK;

    public void DisableIK()
    {
        ikSolver.enabled = false;
        followIK.enabled = false;
    }

    public void EnableIK()
    {
        ikSolver.enabled = true;
        followIK.enabled = true;
    }

    public void MoveIkToPlayer()
    {
        ikTarget.position = playerTransformReference.Value.position;
    }
}
