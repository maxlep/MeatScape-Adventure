using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using MoreMountains.Feedbacks;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using UnityEngine;

public class MeatScale : MonoBehaviour
{
    [SerializeField] private IntReference TargetSize;
    [SerializeField] private IntReference CurrentSize;
    [SerializeField] private Transform BoneRotatePivot;
    [SerializeField] private Transform DishPlayer;
    [SerializeField] private Transform DishElf;
    [SerializeField] private float DishRotDegrees = 30f;
    [SerializeField] private float DishYRange = 100f;
    [SerializeField] private float DishLerpSpeed = 5f;
    [SerializeField] private AudioSource ReelAudioSource;
    [SerializeField] private float ReelThreshold = 1f;

    private float targetWeight;
    private float currentWeight;
    private Vector3 dishPlayerStarPos;
    private Vector3 dishElfStarPos;
    private bool playerIsInside;

    private void Awake()
    {
        dishPlayerStarPos = DishPlayer.position;
        dishElfStarPos = DishElf.position;
    }

    private void Update()
    {
        if (playerIsInside)  //Open cage so player doesnt get stuck
            targetWeight = TargetSize.Value;
        
        currentWeight = Mathf.Lerp(currentWeight, targetWeight, DishLerpSpeed * Time.deltaTime);
        float currentTargetDelta = Mathf.Abs(targetWeight - currentWeight);
        
        float percentToTargetSize = Mathf.InverseLerp(0, TargetSize.Value, currentWeight);
        
        //Manage chain reel audio
        Debug.Log(percentToTargetSize);
        if (currentTargetDelta > ReelThreshold)
        {
            if (!ReelAudioSource.isPlaying)
            {
                ReelAudioSource.Play();
            }
        }
        else
        {
            if (ReelAudioSource.isPlaying)
            {
                ReelAudioSource.Stop();
            }
        }
            
        
        //Rotate Top bone
        float currentRotZ = Mathf.Lerp(-DishRotDegrees, DishRotDegrees, percentToTargetSize);
        BoneRotatePivot.rotation = Quaternion.Euler(BoneRotatePivot.rotation.x, BoneRotatePivot.rotation.y, currentRotZ);
        
        //Move dishes
        float moveDist = Mathf.Lerp(0f, DishYRange, percentToTargetSize);
        DishPlayer.position = dishPlayerStarPos + moveDist * Vector3.down;
        DishElf.position = dishElfStarPos + moveDist * Vector3.up;
    }

    public void AddWeight()
    {
        CurrentSize.Subscribe(RegisterWeight);
        targetWeight = CurrentSize.Value;
    }

    private void RegisterWeight()
    {
        targetWeight = CurrentSize.Value;
    }

    public void RemoveWeight()
    {
        CurrentSize.Unsubscribe(RegisterWeight);
        targetWeight = 0f;
    }

    public void PlayerInside(bool isInside)
    {
        playerIsInside = isInside;

        if (!playerIsInside) targetWeight = 0f;
    }
}
