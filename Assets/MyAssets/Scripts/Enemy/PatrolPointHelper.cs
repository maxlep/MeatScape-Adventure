using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class PatrolPointHelper : MonoBehaviour
{
    [SerializeField] [PropertySpace(5f, 5f)]
    private bool enableGizmos;
    
    [SerializeField] [PropertySpace(5f, 5f)]
    private GameObject patrolPointPrefab;
    
    [SerializeField] [UnityEngine.Tooltip("Patrol points will share parent with this object")]
    private Transform patrolPointSiblingTransform;

    [SerializeField] 
    private bool createPointsAtRuntime;
    
    [SerializeField] [ShowIf("$createPointsAtRuntime")] 
    private int patrolPointCount = 6;
    
    [SerializeField] [ShowIf("$createPointsAtRuntime")]
    private float patrolRadius = 30f;
    
    [SerializeField] [ShowIf("$createPointsAtRuntime")] 
    private LayerMask groundMask;

    [InfoBox("Patrol Points should only be added via buttons. Removing patrol point deletes its GameObject. " +
             "Patrol Points require \"PatrolPoint\" tag (Add button does this for you).")]
    [Required("Null or missing patrol points!")]
    [SerializeField] [ListDrawerSettings(Expanded = true, CustomRemoveElementFunction = "RemovePatrolPoint")] 
    [PropertySpace(10f, 10f)]
    private List<Transform> patrolPoints = new List<Transform>();

    public List<Transform> PatrolPoints => patrolPoints;
    
    private string patrolPointTag = "PatrolPoint";
    private string patrolPointPrefix = "Point";

    #region Validation

    private void ValidatePatrolPoints()
    {
        //Remove points that are null or dont have correct tag
        for (int i = patrolPoints.Count - 1; i > 0; i--)
        {
            if (patrolPoints[i] == null || patrolPoints[i].tag != patrolPointTag)
            {
                patrolPoints.RemoveAt(i);
            }
        }
    }
    
    private void OnValidate()
    {
        ValidatePatrolPoints();
    }

    #endregion

    #region Runtime Patrol Points

    private void Awake()
    {
        if (createPointsAtRuntime)
            SetPatrolPointsRuntime();
    }

    private void SetPatrolPointsRuntime()
    {
        patrolPoints.Clear();
        Vector3 verticalOffset = 1000f * Vector3.up;
        
        for (int i = 0; i < patrolPointCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
            Vector3 raycastOrigin = transform.position + randomOffset.xoy() + verticalOffset;
            AddPatrolPoint(RaycastGround(raycastOrigin));
        }
    }

    private Vector3 RaycastGround(Vector3 raycastOrigin)
    {
        RaycastHit hit;
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, Mathf.Infinity, groundMask))
        {
            return hit.point;
        }
        
        //TODO: Handle failure scenario, for now just returning this transform pos
        return transform.position;
    }

    #endregion
    
    #region Patrol Point Operations
    
    [Button(ButtonSizes.Medium)] [HideIf("$createPointsAtRuntime")]
    private void AddPatrolPoint()
    {
        //Spawn at last point or this transform
        Vector3 newPointPosition = (patrolPoints.Count > 0) ?
            patrolPoints[patrolPoints.Count - 1].position : transform.position;

        AddPatrolPoint(newPointPosition);
    }
    
    private void AddPatrolPoint(Vector3 newPointPosition)
    {
        var newPoint = GameObject.Instantiate(patrolPointPrefab, newPointPosition, 
            Quaternion.identity, patrolPointSiblingTransform.parent);

        newPoint.tag = patrolPointTag;
        newPoint.name = $"{patrolPointPrefix}{patrolPoints.Count + 1}";
        patrolPoints.Add(newPoint.transform);
    }
    
    [Button(ButtonSizes.Medium)] [HideIf("$createPointsAtRuntime")]
    private void RenamePatrolPoints()
    {
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            patrolPoints[i].name = $"{patrolPointPrefix}{i + 1}";
        }
    }
    
    

#if UNITY_EDITOR
    private void RemovePatrolPoint(Transform pointTransform)
    {
        //DestroyImmediate(pointTransform.gameObject);
        Undo.DestroyObjectImmediate(pointTransform.gameObject);
    }
    
#endif

    #endregion


    #region Gizmos

    private void OnDrawGizmos()
    {
        if (!enableGizmos) return;
        
        Draw.LineGeometry = LineGeometry.Volumetric3D;
        Draw.LineThicknessSpace = ThicknessSpace.Meters;
        Draw.LineThickness = .2f;
        Draw.LineDashStyle = DashStyle.DefaultDashStyleLine;

        //Draw spawn sphere
        // Draw.Cuboid(ShapesBlendMode.Transparent, ThicknessSpace.Meters, transform.position, Quaternion.identity, 
        //     Vector3.one * 1.5f, new Color(209f/255f, 130f/255f, 171f/255f, .5f));

        DrawPatrolPathGizmos();
    }

    private void DrawPatrolPathGizmos()
    {
        //Draw dotted line from spawn to first patrol point
        Draw.LineDashed(transform.position, patrolPoints[0].position,
            new Color(150f/255f, 50f/255f, 220f/255f, .5f));
        
        
        //Draw dotted lines with arrows (cones) between patrol points
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            Vector3 startPoint = patrolPoints[i].position;
            Vector3 endPoint;
            Vector3 forward;
            Vector3 coneOffset;
            float coneRadius = .5f;
            float coneHeight = 1.35f;
            float startSphereRadius = 1f;
            Color lineColor = new Color(235f/255f, 201f/255f, 12f/255f, .5f);
            Color coneColor = new Color(235f/255f, 100f/255f, 0f/255f, .5f);
            Color startSphereColor = new Color(181f/255f, 14f/255f, 21f/255f, .5f);
            Color startConeColor = new Color(150f/255f, 50f/255f, 220f/255f, .5f);

            if (i == 0 && patrolPoints.Count > 1)
            {
                Draw.Sphere(startPoint, startSphereRadius, startSphereColor);
                Draw.Cone(transform.position, transform.rotation, coneRadius, coneHeight, startConeColor);
            }

            if (i < patrolPoints.Count - 1)
            {
                endPoint = patrolPoints[i + 1].position;
            }
            else
            {
                endPoint = patrolPoints[0].position;
            }

            //If only 1 point, point cone forward
            if (patrolPoints.Count == 1)
            {
                forward = transform.forward;
                coneOffset = Vector3.zero;
            }
            else
            {
                forward = (endPoint- startPoint).normalized;
                coneOffset = -forward * (coneHeight * 1.15f);
            }

            
            Draw.LineDashed(startPoint, endPoint, lineColor);
            Quaternion coneRot = (!Mathf.Approximately(forward.magnitude, 0f))
                ? Quaternion.LookRotation(forward)
                : Quaternion.identity;
            Draw.Cone(endPoint + coneOffset, coneRot, coneRadius, coneHeight, coneColor);
        }
    }

    
    

    #endregion
}
