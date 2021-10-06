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
    [SerializeField] private float LeanAngle;

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

        Vector3 curveRight = Vector3.Cross(tangent, Vector3.up);
        Vector3 tiltedTangent = Quaternion.AngleAxis(-LeanAngle, curveRight) * tangent;
        Agent.rotation = Quaternion.LookRotation(tiltedTangent);
    }
}
