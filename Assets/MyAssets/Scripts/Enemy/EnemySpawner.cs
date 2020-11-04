using System;
using System.Collections.Generic;
using System.Linq;
using AmplifyShaderEditor;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCharacterController;
using MyAssets.ScriptableObjects.Variables;
using Shapes;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

public class EnemySpawner : MonoBehaviour
{
    [SuffixLabel("seconds", Overlay = true)] [SerializeField] 
    private float SpawnTime = 1;
    
    [SerializeField] [ValueDropdown("GetEnemyDropdown")] [OnValueChanged("StoreEnemyPrefabMeshes")]
    private GameObject EnemyPrefab;
    
    [SerializeField] [PropertySpace(5f, 5f)]
    private EnemyTypeContainer EnemyTypes;
    
    [SerializeField] [PropertySpace(5f, 5f)]
    private GameObject PatrolPointPrefab;

    [InfoBox("Patrol Points should only be added via buttons. Removing patrol point deletes its GameObject. " +
             "Patrol Points require \"PatrolPoint\" tag (Add button does this for you).")]
    [Required("Null or missing patrol points!")]
    [SerializeField] [ListDrawerSettings(Expanded = true, CustomRemoveElementFunction = "RemovePatrolPoint")] 
    private List<Transform> PatrolPoints = new List<Transform>();
    
    private List<(Transform transform, Mesh mesh)> enemyTransformMeshTuples = new List<(Transform transform, Mesh mesh)>();
    private string patrolPointTag = "PatrolPoint";
    private string patrolPointPrefix = "Point";

    #region Inspector/Odin Methods

    private List<GameObject> GetEnemyDropdown()
    {
        return EnemyTypes.EnemyPrefabs;
    }
    
    private void ValidatePatrolPoints()
    {
        //Remove points that are null or dont have correct tag
        for (int i = PatrolPoints.Count - 1; i > 0; i--)
        {
            if (PatrolPoints[i] == null || PatrolPoints[i].tag != patrolPointTag)
            {
                PatrolPoints.RemoveAt(i);
            }
        }
    }

    [Button(ButtonSizes.Medium)]
    private void AddPatrolPoint()
    {
        //Spawn at last point or this transform
        Vector3 newPointPosition = (PatrolPoints.Count > 0) ?
            PatrolPoints[PatrolPoints.Count - 1].position : transform.position;

        var newPoint = GameObject.Instantiate(PatrolPointPrefab, newPointPosition, 
            Quaternion.identity, transform);

        newPoint.tag = patrolPointTag;
        newPoint.name = $"{patrolPointPrefix}{PatrolPoints.Count + 1}";
        PatrolPoints.Add(newPoint.transform);
    }
    
    [Button(ButtonSizes.Medium)]
    private void RenamePatrolPoints()
    {
        for (int i = 0; i < PatrolPoints.Count; i++)
        {
            PatrolPoints[i].name = $"{patrolPointPrefix}{i + 1}";
        }
    }

    private void RemovePatrolPoint(Transform pointTransform)
    {
        //DestroyImmediate(pointTransform.gameObject);
        Undo.DestroyObjectImmediate(pointTransform.gameObject);
    }

    #endregion
    
    
    private void Awake()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy() {
        GameObject enemy = Instantiate(EnemyPrefab, transform.position, transform.rotation, transform);
        EnemyController enemyScript = enemy.GetComponent<EnemyController>();
        
        enemyScript.Initialize(PatrolPoints);
        enemyScript.OnDeath += () => {
            LeanTween.value(0f, 1f, SpawnTime).setOnComplete(_ => SpawnEnemy());
        };
    }
    
    private void OnValidate()
    {
        if (enemyTransformMeshTuples.IsNullOrEmpty())
        {
            StoreEnemyPrefabMeshes();
        }

        ValidatePatrolPoints();
    }

    #region Gizmos

    private void OnDrawGizmos()
    {
        Draw.LineGeometry = LineGeometry.Volumetric3D;
        Draw.LineThicknessSpace = ThicknessSpace.Meters;
        Draw.LineThickness = .2f;
        Draw.LineDashStyle = DashStyle.DefaultDashStyleLine;

        //Draw spawn sphere
        // Draw.Cuboid(ShapesBlendMode.Transparent, ThicknessSpace.Meters, transform.position, Quaternion.identity, 
        //     Vector3.one * 1.5f, new Color(209f/255f, 130f/255f, 171f/255f, .5f));

        DrawPatrolPathGizmos();
        DrawEnemyMeshGizmo();
    }

    private void DrawPatrolPathGizmos()
    {
        //Draw dotted line from spawn to first patrol point
        Draw.LineDashed(transform.position, PatrolPoints[0].position,
            new Color(150f/255f, 50f/255f, 220f/255f, .5f));
        
        
        //Draw dotted lines with arrows (cones) between patrol points
        for (int i = 0; i < PatrolPoints.Count; i++)
        {
            Vector3 startPoint = PatrolPoints[i].position;
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

            if (i == 0 && PatrolPoints.Count > 1)
            {
                Draw.Sphere(startPoint, startSphereRadius, startSphereColor);
                Draw.Cone(transform.position, transform.rotation, coneRadius, coneHeight, startConeColor);
            }

            if (i < PatrolPoints.Count - 1)
            {
                endPoint = PatrolPoints[i + 1].position;
            }
            else
            {
                endPoint = PatrolPoints[0].position;
            }

            //If only 1 point, point cone forward
            if (PatrolPoints.Count == 1)
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

    private void DrawEnemyMeshGizmo()
    {
        foreach (var (meshTransform, mesh) in enemyTransformMeshTuples)
        {
            Vector3 position = meshTransform.position + transform.position;
            Quaternion rotation = meshTransform.rotation;
            Vector3 scale = meshTransform.lossyScale;
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
            
            
            if (!Application.isPlaying) 
                Gizmos.color = new Color(.5f, .5f, .5f, .5f);
            else
                Gizmos.color = new Color(.5f, .5f, .5f, .25f);
            
            
            Gizmos.DrawMesh(mesh, position, rotation, scale);
            
            
                
        }
    }

    private void StoreEnemyPrefabMeshes()
    {
        if (EnemyPrefab == null) return;
        
        enemyTransformMeshTuples.Clear();

        MeshFilter[] meshFilters = EnemyPrefab.GetComponentsInChildren<MeshFilter>();

        foreach (var meshFilter in meshFilters)
        {
            enemyTransformMeshTuples.Add((meshFilter.transform, meshFilter.sharedMesh));
        }
    }
    

    #endregion

    
}
