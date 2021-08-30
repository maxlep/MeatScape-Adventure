using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using MyAssets.Scripts.Utils;
using UnityEngine;

public class GlideAudioController : MonoBehaviour
{
    [SerializeField] private Vector3Reference PreviousVelocity;
    [SerializeField] private BandPassFilter filter;
    [SerializeField] private AudioSource source;
    [SerializeField] private float maxVolume = .5f;
    [SerializeField] private float sweepMin = .3f;
    [SerializeField] private float sweepMax = .9f;
    [SerializeField] private float minVelocity = 0f;
    [SerializeField] private float maxVelocity = 120f;  //TODO: Make this the base speed for glide?
    [SerializeField] private float minAngle = -60f;
    [SerializeField] private float maxAngle = 60f;

    private void Update()
    {
        //Set volume based on percent to max velocity
        float percentToMaxVel = Mathf.InverseLerp(minVelocity, maxVelocity, PreviousVelocity.Value.magnitude);
        source.volume = Mathf.Lerp(0f, maxVolume, percentToMaxVel);
        
        //Adjust the band filter's sweep center based on angle of velocity
        var playerRight = Vector3.Cross(PreviousVelocity.Value.xoz().normalized, Vector3.up);
        float angleBelowHorizontal = Vector3.SignedAngle(PreviousVelocity.Value.xoz().normalized,
            PreviousVelocity.Value.normalized, playerRight);
        float percentToMaxAngle = Mathf.InverseLerp(minAngle, maxAngle, angleBelowHorizontal);
        float sweepAmount = Mathf.Lerp(sweepMin, sweepMax, percentToMaxAngle);
        filter.SetBandCenter(sweepAmount);
    }
}
