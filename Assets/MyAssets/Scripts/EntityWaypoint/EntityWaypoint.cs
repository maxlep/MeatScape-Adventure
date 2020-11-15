using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using XNodeEditor;

public class EntityWaypoint : MonoBehaviour
{
    [SerializeField] private TransformSceneReference playerCameraTrans;
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private RectTransform waypointContainerTrans;
    [SerializeField] private RectTransform pointerImageTrans;
    [SerializeField] private bool scaleWithDistance = true;
    [SerializeField] private float minScaleFactor = .25f;
    [SerializeField] private float maxDistance = 500f;
    [SerializeField] private float minVisibleDistance = 20f;
    [SerializeField] private bool showOffscreen = true;
    [SerializeField] [Range(0f, 1f)] private float edgeOfScreenPercentOffset = .1f;
    [SerializeField] private bool showOutOfSight = true;
    [SerializeField] [ShowIf("showOutOfSight")] private LayerMask lineOfSightMask;

    private Camera playerCam;
    private Vector3 startScale;
    private Vector3 camToTarget;
    private Vector2 camToTargetScreenDir;
    private float distanceFromCam;
    private bool isOffScreen;

    private void Awake()
    {
        playerCam = playerCameraTrans.Value.GetComponent<Camera>();
        startScale = waypointContainerTrans.localScale;
    }

    private void Update()
    {
        ToggleImageActive(CheckIfVisible());
        CalculateScreenDirection();
        Follow();
        RotatePointerToTarget();
        if (scaleWithDistance) ScaleWithDistance();
    }

    private void CalculateScreenDirection()
    {
        //Get dir from cam to target and project onto cam plane normalized
        distanceFromCam = (followTarget.position - playerCameraTrans.Value.position).magnitude;
        camToTarget = (followTarget.position - playerCameraTrans.Value.position).normalized;
        Vector3 projectOnCameraPlane = Vector3.ProjectOnPlane(camToTarget, 
            playerCameraTrans.Value.forward);
        Vector3 camToTargetLocalDir = playerCameraTrans.Value.InverseTransformVector(projectOnCameraPlane);
        camToTargetScreenDir = new Vector2(camToTargetLocalDir.x, camToTargetLocalDir.y).normalized;
    }

    private void Follow()
    {
        Vector3 targetScreenPos = playerCam.WorldToScreenPoint(followTarget.position + followOffset);
        
        //If waypoint is not in camera's view
        if (!Screen.safeArea.Contains(targetScreenPos) || targetScreenPos.z < 0f)
        {
            isOffScreen = true;

            //Get distance from center of screen rectangle to point on edge
            //in direction of camToTargetScreenDir
            float distanceToEdge =
                MathUtils.GetDistanceToRectangleEdgeFromCenter(Screen.width, Screen.height, camToTargetScreenDir);
            distanceToEdge *= (1f - edgeOfScreenPercentOffset);
            
            Vector2 centerScreen = new Vector2(Screen.width/2f, Screen.height/2f);
            targetScreenPos = centerScreen + camToTargetScreenDir * distanceToEdge;
        }
        else
        {
            isOffScreen = false;
        }
        
        waypointContainerTrans.position = targetScreenPos;
    }

    private void ScaleWithDistance()
    {
        float distanceBasedFactor =  Mathf.Lerp(1f, 0f, distanceFromCam / maxDistance);
        waypointContainerTrans.localScale = startScale * Mathf.Max(distanceBasedFactor, minScaleFactor);
    }

    private bool CheckIfVisible()
    {
        //Show off screen
        if (showOffscreen && isOffScreen)
            return true;
        
        //MinVisibleDistance
        if (distanceFromCam >= minVisibleDistance) 
            return true;

        //Line of Sight
        RaycastHit hit;
        if (showOutOfSight && 
            Physics.Raycast(playerCameraTrans.Value.position, camToTarget.normalized, out hit,
                distanceFromCam, lineOfSightMask))
        {
            return true;
        }

        return false;
    }

    private void RotatePointerToTarget()
    {
        if (pointerImageTrans == null) return;

        if (isOffScreen)
            pointerImageTrans.rotation = Quaternion.Euler(0f, 0f, -Vector2.SignedAngle(camToTargetScreenDir, Vector2.down));
        else
            pointerImageTrans.rotation = Quaternion.identity;
    }

    private void ToggleImageActive(bool value)
    {
        if (value && !waypointContainerTrans.gameObject.activeSelf) waypointContainerTrans.gameObject.SetActive(value);
        if (!value && waypointContainerTrans.gameObject.activeSelf) waypointContainerTrans.gameObject.SetActive(value);
    }
}
