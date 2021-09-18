using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using RootMotion.FinalIK;
using UnityEngine;

public class AssignLookAtTarget : MonoBehaviour
{
    [SerializeField] private TransformSceneReference lookAtTarget;
    [SerializeField] private BoolReference lockedOn;
    [SerializeField] private LookAtIK lookAtIK;

    private void Update()
    {
        if (lockedOn.Value)
        {
            lookAtIK.solver.target = lookAtTarget.Value;
            lookAtIK.solver.IKPositionWeight = 1f;
        }

        else
        {
            lookAtIK.solver.target = null;
            lookAtIK.solver.IKPositionWeight = 0f;
        }
    }
}
