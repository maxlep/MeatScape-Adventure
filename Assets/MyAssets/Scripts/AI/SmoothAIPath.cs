using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.Scripts.Utils;
using Pathfinding;
using Shapes;
using UnityEngine;

public class SmoothAIPath : MonoBehaviour
{
    [SerializeField] private Transform Destination;
    [SerializeField] private Seeker Seeker;
    [SerializeField] private CharacterController CharController;
    [SerializeField] private LayerMask GroundMask;
    [SerializeField] private LayerMask EnemyMask;
    [SerializeField] private Collider HomeTrigger;
    [SerializeField] private TransformSceneReference HomeContainerRef;
    [SerializeField] private float PathRequestDelay = .1f;
    [SerializeField] private float StoppingDistance = 5f;
    [SerializeField] private float MoveSpeedX = 10f;
    [SerializeField] private float MoveSpeedY = 3f;
    [SerializeField] private float TurnSpeed = 3f;
    [SerializeField] private float MinTurnAngle = 20f;
    [SerializeField] private float GroundOffset = 15f;
    [SerializeField] private float AvoidEnemyVelocity = 5f;
    [SerializeField] private float AvoidEnemyRadius = 25f;

    public Transform Home => home;
    public bool IsHome => isHome;

    public List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex;
    private float lastRequestTime = Mathf.NegativeInfinity;
    private bool isStopped;
    private bool isHome;
    private float sqrStoppingDistance;
    private Transform home;

    private void Start ()
    {
        RequestPath();
        sqrStoppingDistance = Mathf.Pow(StoppingDistance, 2);
        
        //Create home transform at spawn position
        home = new GameObject("Home").transform;
        if (HomeContainerRef != null) home.transform.parent = HomeContainerRef.Value;
        home.transform.position = transform.position;
        isHome = true;
    }

    private void Update()
    {
        if (lastRequestTime + PathRequestDelay < Time.time)
            RequestPath();
        
        if (currentPath == null || currentPath.Count == 0) return;

        CheckReachedNextPoint();
        Move();
    }

    private void RequestPath()
    {
        if (Destination == null) return;
        
        Seeker.StartPath (transform.position, Destination.position, OnPathComplete);
        lastRequestTime = Time.time;
    }

    private void ValidatePath(List<Vector3> path)
    {
        List<Vector3> validatedPath = new List<Vector3>();
        
        if (HomeTrigger != null)
        {
            //Remove path points outside of home trigger
            foreach (var pathPoint in path)
            {
                Vector3 closestPointInHome = HomeTrigger.ClosestPoint(pathPoint);
                if (closestPointInHome == pathPoint)
                {
                    validatedPath.Add(pathPoint);
                }
                else
                {
                    //Move point to closest point in home and stop path
                    validatedPath.Add(closestPointInHome);
                    break;
                }
            } 
        }
        
        
        //Remove path points already within stopping distance
        foreach (var pathPoint in path)
        {
            if (!HasReachedPoint(pathPoint))
                validatedPath.Add(pathPoint);
        }

        currentPath = validatedPath;
        currentPathIndex = 0;
    }

    public void OnPathComplete (Path p) {
        // We got our path back
        if (p.error) {
            // Nooo, a valid path couldn't be found
            //RequestPath();
            //currentPath = null;
        } else
        {
            ValidatePath(p.vectorPath);
        }
    }

    private void CheckReachedNextPoint()
    {
        //If within stopping distance of next point, increment index
        //For now ignoring the y pos when checking if reached destination.
        var nextPoint = currentPath[currentPathIndex];
        if (HasReachedPoint(nextPoint))
            currentPathIndex = Mathf.Min(currentPathIndex + 1, currentPath.Count - 1);
        
    }

    private bool HasReachedPoint(Vector3 point)
    {
        //If within stopping distance of point
        //For now ignoring the y pos when checking if reached destination.
        var sqrDistanceIgnoreY = transform.position.SqrDistanceIgnoreY(point);
        return (sqrDistanceIgnoreY <= sqrStoppingDistance);
    }

    private void Move()
    {
        if (isStopped) return;
        if (currentPathIndex == currentPath.Count - 1) return;

        Vector3 deltaMove = Vector3.zero;
        Vector3 vecToNextPoint = (currentPath[currentPathIndex] - transform.position);

        //Flatten rot, then Rotate Towards Target
        transform.rotation = Quaternion.LookRotation(transform.forward.xoz(), Vector3.up);
        Quaternion targetRot = Quaternion.LookRotation(vecToNextPoint.xoz(), Vector3.up);

        //Increase trun speed as the angle to target rot increases [0 deg-> 180 deg] -> [1x, 2x]
        float angleToTarget = Vector3.Angle(transform.forward.xoz(), vecToNextPoint.xoz());
        float angleFactor = Mathf.InverseLerp(0f, 180f, angleToTarget);
        float turnSpeedMult = Mathf.Lerp(.5f, 1.5f, angleFactor);
        
        float turnSpeed = TurnSpeed * turnSpeedMult;
        if (angleToTarget < MinTurnAngle) turnSpeed = 0f;
        
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed);
        
        //Move position Horizontally
        Vector3 horizontalVelocity = transform.forward * MoveSpeedX;
        deltaMove += horizontalVelocity * Time.deltaTime;
        
        //Check for ground below and maintain offset
        (bool foundGround, Vector3 groundLocation) = CheckForGround();

        if (foundGround)
        {
            //Move position Vertically towards target offset from ground
            Vector3 targetOffset = groundLocation + Vector3.up * GroundOffset;

            //Use MoveTowards to find point want to move to (given move delta), then get delta to that point
            Vector3 targetOffsetTowards =
                Vector3.MoveTowards(transform.position, targetOffset, MoveSpeedY * Time.deltaTime);
            deltaMove.y += (targetOffsetTowards - transform.position).y;
        }
        
        //Avoid other enemies
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, AvoidEnemyRadius, EnemyMask,
            QueryTriggerInteraction.Ignore);

        if (enemyColliders.Length > 0)
        {
            Vector3 averagePosition = Vector3.zero;
            foreach (var enemyCollider in enemyColliders)
            {
                averagePosition += enemyCollider.transform.position;
            }

            averagePosition /= enemyColliders.Length;

            Vector3 dirAwayFromEnemies = (transform.position - averagePosition).normalized;
            deltaMove += dirAwayFromEnemies * (AvoidEnemyVelocity * Time.deltaTime);
        }

        CharController.Move(deltaMove);
    }

    private (bool, Vector3) CheckForGround()
    {
        RaycastHit groundHit;
        Vector3 offset = Vector3.up * 10f;
        Ray groundRay = new Ray(transform.position + offset, Vector3.down);
        
        if (Physics.Raycast(groundRay, out groundHit, Mathf.Infinity, GroundMask, QueryTriggerInteraction.Ignore))
        {
            return (true, groundHit.point);
        }
        else
        {
            return (false, Vector3.zero);
        }
    }

    public void SetDestination(Transform newDestination)
    {
        isStopped = false;    //Start moving if again if dest gets set
        Destination = newDestination;
    }

    public void Stop()
    {
        isStopped = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (HomeTrigger == null) return;
        
        //If enter home trigger
        if (other.gameObject.transform.Equals(HomeTrigger.transform))
            isHome = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (HomeTrigger == null) return;

        //If exit home trigger
        if (other.gameObject.transform.Equals(HomeTrigger.transform))
            isHome = false;
    }

    private void OnDrawGizmos()
    {
        // set up all static parameters. these are used for all following Draw.Line calls
        Draw.LineGeometry = LineGeometry.Volumetric3D;
        Draw.LineThicknessSpace = ThicknessSpace.Meters;
        Draw.LineThickness = .1f;

        if (currentPath == null || currentPath.Count == 0) return;

        for (int i = currentPathIndex; i < currentPath.Count; i++)
        {
            Vector3 startPos = (i == currentPathIndex) ? transform.position : currentPath[i - 1];
            Vector3 endPos = (i == currentPath.Count - 1) ? currentPath[i] : currentPath[i + 1];
            Draw.Line(startPos, endPos, new Color(.7f, .5f, .8f, .35f));
        }
    }
}
