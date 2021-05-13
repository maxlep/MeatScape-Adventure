using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeatWormAnimProxy : MonoBehaviour
{
    [SerializeField] private TestWormController controller;
    
    public void DisableIK()
    {
        controller.DisableIK();
    }

    public void EnableIK()
    {
        controller.EnableIK();
    }
}
