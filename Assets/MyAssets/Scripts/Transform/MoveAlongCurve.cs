using System;
using BansheeGz.BGSpline.Components;
using DotLiquid.Util;
using Unity.Mathematics;
using UnityEngine;


public class MoveAlongCurve : MonoBehaviour
{
    [SerializeField] private GameObject CurveObject;
    [SerializeField] private Transform Agent;
    [SerializeField] private float Speed = 20f;
    [SerializeField] private float InitialOffsetDistance = 0f;

    private BGCcMath bGcMath;
    private float distanceTraveled;

    private void Awake()
    {
        bGcMath = CurveObject.GetComponent<BGCcMath>();
        distanceTraveled = InitialOffsetDistance;
    }

    private void Update()
    {
        distanceTraveled += Speed * Time.deltaTime;
        distanceTraveled %= bGcMath.GetDistance();  //Wrap
        
        Vector3 tangent;
        Agent.position = bGcMath.CalcPositionAndTangentByDistance(distanceTraveled, out tangent);
        Agent.rotation = Quaternion.LookRotation(tangent);
    }
}
