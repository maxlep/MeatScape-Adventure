using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCenterOfMass : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    
    void Start()
    {
        rigidbody.centerOfMass = transform.position;
    }


}
