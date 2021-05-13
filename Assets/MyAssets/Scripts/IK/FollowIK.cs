using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using UnityEngine;

/// <summary>
/// This script is for having deform bones follow bones that are affected by IK
/// It caches the IK target bones positions in the preCull callbakc (after IK is done)
/// then it sets the follow bones to those transforms in the next LateUpdate
/// As a result, there is 1 frame of cold start before it works
/// </summary>
public class FollowIK : MonoBehaviour
{
    [SerializeField] private List<Transform> followBones = new List<Transform>();
    [SerializeField] private List<Transform> targetIKBones = new List<Transform>();
    [SerializeField] private GameEvent OnPreCullEvent;
    
    private List<(Vector3 pos, Quaternion rot, Vector3 scale)> previousTargetTransforms = 
        new List<(Vector3 pos, Quaternion rot, Vector3 scale)>();

    private bool coldStart = true;

    private void Awake()
    {
        //Init previousTargetTransform list to size of IK bone list
        foreach (var targetBone in targetIKBones)
        {
            previousTargetTransforms.Add((Vector3.zero, Quaternion.identity, Vector3.zero));
        }
    }

    private void OnEnable()
    {
        OnPreCullEvent.Subscribe(CacheIKBoneTransforms);
    }
    
    private void OnDisable()
    {
        OnPreCullEvent.Unsubscribe(CacheIKBoneTransforms);
        coldStart = true;
    }

    private void LateUpdate()
    {
        //Skip first frame where bone transforms are stale
        if (coldStart) return;
        
        for (int i = 0; i < targetIKBones.Count; i++)
        {
            followBones[i].position = previousTargetTransforms[i].pos;
            followBones[i].rotation = previousTargetTransforms[i].rot;
            followBones[i].localScale = previousTargetTransforms[i].scale;
        }
    }


    //Store the IK bone transforms from last pre-cull
    private void CacheIKBoneTransforms()
    {
        for (int i = 0; i < previousTargetTransforms.Count; i++)
        {
            previousTargetTransforms[i] = (targetIKBones[i].position, targetIKBones[i].rotation,
                targetIKBones[i].localScale);
        }

        if (coldStart) coldStart = false;
    }
}
