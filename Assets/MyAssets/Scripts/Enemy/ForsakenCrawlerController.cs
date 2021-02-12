using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForsakenCrawlerController : EnemyController
{
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask playerMask;

    public bool IsLunging
    {
        get => isLunging;
        set => isLunging = value;
    }
    
    private bool isLunging;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject hitObject = hit.gameObject;
        if (hitObject.IsInLayerMask(playerMask) && isLunging)
        {
            //TODO: Damage player and destory self
            Vector3 dirToPlayer = (hit.gameObject.transform.position - transform.position).normalized;
            
            PlayerController playerScript = hitObject.GetComponent<PlayerController>();
            playerScript.Damage(damage, dirToPlayer, 10f);
        }
    }
}
