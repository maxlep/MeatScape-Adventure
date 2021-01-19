using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimMoveBool : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private CharacterController charController;
    [SerializeField] private string parameterName = "IsWalking";
    [SerializeField] private float moveThreshold = 1f;

    private void Update()
    {
        if (charController.velocity.sqrMagnitude >= Mathf.Pow(moveThreshold, 2f))
        {
            anim.SetBool(parameterName, true);
        }
        else
        {
            anim.SetBool(parameterName, false);
        }
    }
}
