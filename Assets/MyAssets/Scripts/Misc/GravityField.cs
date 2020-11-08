using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [SerializeField] private LayerMapper LayerMapper;
    [SerializeField] private FloatReference StoredJumpVelocity;
    [SerializeField] private float Force = 1f;
    [SerializeField] private bool UseStoredJumpVelocity = false;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            Vector3 forceDir = (transform.position - playerController.transform.position).normalized;

            if (!UseStoredJumpVelocity)
            {
                playerController.UngroundMotor();
                playerController.AddVelocity(forceDir * Force);
            }
            else
            {
                playerController.UngroundMotor();
                StoredJumpVelocity.Value = Force;
            }
        }
    }
}