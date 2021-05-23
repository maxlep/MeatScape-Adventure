using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForsakenCrawlerController : EnemyController
{
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask playerMask;

    public bool IsAttacking
    {
        get => isAttacking;
        set => isAttacking = value;
    }
    
    private bool isAttacking;

    public void HandleCollisions(Collider[] colliders) {
        if (colliders.Length < 1) return;
        foreach (var collider in colliders) {
            GameObject hitObject = collider.gameObject;
            if (hitObject.IsInLayerMask(playerMask) && IsAttacking) {
                Vector3 dirToPlayer = (collider.gameObject.transform.position - transform.position).normalized;
                
                PlayerController playerScript = hitObject.GetComponent<PlayerController>();
                playerScript.Damage(damage, dirToPlayer, 10f);
                IsAttacking = false;
                return;
            }
        }
    }
}
