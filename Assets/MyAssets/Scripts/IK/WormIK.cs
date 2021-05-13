using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormIK : MonoBehaviour
{
    public Animator anim;
    public List<Transform> followBones = new List<Transform>();
    public List<Transform> targetBones = new List<Transform>();
    
    
    private void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("test");

        for (int i = 0; i < followBones.Count; i++)
        {
            followBones[i].position = targetBones[i].position;
            followBones[i].rotation = targetBones[i].rotation;
            followBones[i].localScale = targetBones[i].localScale;
        }
    }
}
