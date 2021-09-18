using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentChainFollow : MonoBehaviour
{
    [SerializeField] private float MinDistance;
    [SerializeField] private List<Transform> SegmentList;


    private void Update()
    {
        Move();
    }

    private void Move()
    {

        Vector3 currentVelocity = SegmentList[0].up * Mathf.Sin(Time.time) * 10f + 
                                  SegmentList[0].forward * 10f;
        Vector3 deltaMove = currentVelocity * Time.smoothDeltaTime;
        

        SegmentList[0].Translate(deltaMove, Space.World);
        

        for (int i = 1; i < SegmentList.Count; i++)
        {

            Transform curBodyPart = SegmentList[i];
            Transform PrevBodyPart = SegmentList[i - 1];

            float dis = Vector3.Distance(PrevBodyPart.position,curBodyPart.position);

            float T = Time.deltaTime * dis / MinDistance * currentVelocity.magnitude;

            if (T > 0.5f)
                T = 0.5f;
            
            curBodyPart.position = Vector3.Slerp(curBodyPart.position, PrevBodyPart.position, T);
            //curBodyPart.rotation = Quaternion.Slerp(curBodyPart.rotation, PrevBodyPart.rotation, T);
            curBodyPart.rotation = Quaternion.LookRotation(deltaMove.normalized);
        }
    }
}
