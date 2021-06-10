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
    [SerializeField] private float PathRequestDelay = .1f;
    [SerializeField] private float StoppingDistance = 5f;
    [SerializeField] private float MoveSpeedX = 10f;
    [SerializeField] private float MoveSpeedY = 3f;
    [SerializeField] private float TurnSpeed = 3f;
    [SerializeField] private float GroundOffset = 15f;
    [SerializeField] private float AvoidEnemyVelocity = 5f;
    [SerializeField] private float AvoidEnemyRadius = 25f;

    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex;
    private float lastRequestTime = Mathf.NegativeInfinity;
    private bool isStopped;
    
    private void Start ()
    {
        RequestPath();
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

    public void OnPathComplete (Path p) {
        // We got our path back
        if (p.error) {
            // Nooo, a valid path couldn't be found
            RequestPath();
        } else {
            currentPath = p.vectorPath;
            currentPathIndex = 0;
        }
    }

    private void CheckReachedNextPoint()
    {
        //If within stopping distance of next point, increment index
        //For now ignoring the y pos when checking if reached destination
        if (Vector3.Distance(transform.position.xoz(), currentPath[currentPathIndex].xoz()) <= StoppingDistance)
            currentPathIndex = Mathf.Min(currentPathIndex + 1, currentPath.Count - 1);
    }

    private void Move()
    {
        if (isStopped) return;

        Vector3 deltaMove = Vector3.zero;
        Vector3 dirToNextPoint = (currentPath[currentPathIndex] - transform.position).normalized;

        //Flatten rot, then Rotate Towards Target
        transform.rotation = Quaternion.LookRotation(transform.forward.xoz(), Vector3.up);
        Quaternion targetRot = Quaternion.LookRotation(dirToNextPoint.xoz(), Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, TurnSpeed);
        
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
            Vector3 deltaMoveY = (targetOffsetTowards - transform.position).oyo();
            deltaMove += deltaMoveY;
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
            deltaMove += dirAwayFromEnemies * AvoidEnemyVelocity * Time.deltaTime;
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
            Draw.Line(startPos, endPos, new Color(.3f, 1f, .8f, .35f));
        }
    }
}
