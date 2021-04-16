using System;
using System.Collections;
using System.Collections.Generic;
using MapMagic.Core;
using MeshCombineStudio;
using MyAssets.Scripts.Utils;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MeshCombineRuntimeCombiner : MonoBehaviour
{
    public TransformSceneReference playerTransform;
    public MeshCombiner MeshCombiner;
    public MapMagicObject mapMagic;
    public float distCheckDelay = 3f;

    private float lastDistCheckTime;
    private Vector3 lastUpdatePos;

    private float sqrDistFromLastUpdate;

    private void Start()
    {
        lastUpdatePos = playerTransform.Value.position;
        lastDistCheckTime = Time.time;
    }

    private void Update()
    {
        //Dont check every frame
        if (Time.time - lastDistCheckTime < distCheckDelay) return;

        lastDistCheckTime = Time.time;
        sqrDistFromLastUpdate = (playerTransform.Value.position - lastUpdatePos).xoz().sqrMagnitude;
        
        if (sqrDistFromLastUpdate > Mathf.Pow(mapMagic.tileSize.x,2f))
        {
            MeshCombiner.CombineAll();
            lastUpdatePos = playerTransform.Value.position;
            Debug.Log($"Combining All {Time.time}");
        }
    }
}
