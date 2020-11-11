using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class Bouncy : MonoBehaviour
{
    [SerializeField] private LayerMapper LayerMapper;
    [SerializeField] private FloatReference StoredJumpVelocity;
    [SerializeField] private float BounceVelocity = 30f;
    [SerializeField] private bool UseStoredJumpVelocity = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            
            if (!UseStoredJumpVelocity)
            {
                playerController.UngroundMotor();
                playerController.AddImpulse(Vector3.up * BounceVelocity);
            }
            else
            {
                playerController.UngroundMotor();
                StoredJumpVelocity.Value = BounceVelocity;
            }
        }
    }
}
