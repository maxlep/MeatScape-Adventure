using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ForceEffector : MonoBehaviour
{
    [SerializeField] private ForceType forceType;
    [SerializeField] private LayerMapper LayerMapper;
    [SerializeField] private bool isRadialForce = false;
    [SerializeField] [HideIf("isRadialForce")] private Vector3 direction; //toggle local/world space?
    [SerializeField] [ShowIf("isRadialForce")] private Vector3 targetPoint; //toggle local/world space?
    [SerializeField] private float forceMagnitude;
    //Toggle for stored jump vel?
    //Toggle for additive or set (set in all components)? If so, need new SO for set impulse, but then applies in all states?

    [Serializable]
    private enum ForceType
    {
        Constant,
        Impulse
    }
    
    //TODO: Horiz. force has to be enough to get player out of trigger since additive?
    
    private void OnTriggerEnter(Collider other)
    {
        if (forceType != ForceType.Impulse) return;
        
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            ApplyForceToPlayer(playerController);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (forceType != ForceType.Constant) return;
        
        if (other.gameObject.layer == LayerMapper.GetLayer(LayerEnum.Player))
        {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            ApplyForceToPlayer(playerController);
        }
    }

    private void ApplyForceToPlayer(PlayerController playerController)
    {
        playerController.UngroundMotor();

        if (isRadialForce)
        {
            Vector3 radialForceDir = (targetPoint - playerController.transform.position).normalized;
            playerController.AddImpulse(radialForceDir * forceMagnitude);
        }
        else
        {
            playerController.AddImpulse(direction * forceMagnitude);
        }
    }

    private void OnDrawGizmosSelected()
    {
        //TODO: Gizmos to show the forces and directions when selected
        //TODO: Utilize handles to modify properties (Force dir, forceTargetPoint, etc)
    }
}
