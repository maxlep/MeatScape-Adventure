using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharControllerCollisionProxy : MonoBehaviour
{
    public UnityEvent<ControllerColliderHit> CollisionResponse;
    
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CollisionResponse.Invoke(hit);
    }
}
