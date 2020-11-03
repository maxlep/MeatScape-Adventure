using System;
using System.Collections.Generic;
using AmplifyShaderEditor;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCharacterController;
using MyAssets.ScriptableObjects.Variables;
using Shapes;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SuffixLabel("seconds", Overlay = true)] [SerializeField] private float spawnTime = 300;
    
    [Required("Null or missing patrol points! Will default to just spawn point.")]
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();

    private void Awake()
    {
        ValidatePatrolPoints();
        SpawnEnemy();
    }
    
    private void ValidatePatrolPoints()
    {
        if (patrolPoints.IsNullOrEmpty())
        {
            patrolPoints = new List<Transform>();
            patrolPoints.Add(transform);
            return;
        }

        foreach (var patrolPoint in patrolPoints)
        {
            if (patrolPoint == null)
            {
                patrolPoints = new List<Transform>();
                patrolPoints.Add(transform);
                return;
            }
        }
    }

    private void SpawnEnemy() {
        GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation, transform);
        EnemyController enemyScript = enemy.GetComponent<EnemyController>();
        
        enemyScript.Initialize(patrolPoints);
        enemyScript.OnDeath += () => {
            LeanTween.value(0f, 1f, spawnTime).setOnComplete(_ => SpawnEnemy());
        };
    }

    private void OnDrawGizmos()
    {
        Draw.LineGeometry = LineGeometry.Volumetric3D;
        Draw.LineThicknessSpace = ThicknessSpace.Pixels;
        Draw.LineThickness = 3f;
        Draw.LineDashStyle = DashStyle.DefaultDashStyleLine;

        //Draw spawn cuboid
        Draw.Cuboid(ShapesBlendMode.Transparent, ThicknessSpace.Meters, transform.position, Quaternion.identity, 
            Vector3.one * 1.5f, new Color(209f/255f, 130f/255f, 171f/255f, .5f));

        //Draw patrol paths
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            Vector3 startPoint = patrolPoints[i].position;
            Vector3 endPoint;
            Vector3 forward;
            Vector3 coneOffset;
            float coneRadius = .75f;
            float coneHeight = 2f;
            float sphereRadius = .75f;
            float startSphereRadius = 1.5f;
            Color lineColor = new Color(235f/255f, 201f/255f, 12f/255f, .5f);
            Color sphereColor = new Color(20f/255f, 20f/255f, 17f/255f, .5f);
            Color startSphereColor = new Color(181f/255f, 14f/255f, 21f/255f, .5f);

            if (i == 0)
            {
                sphereColor = startSphereColor;
                sphereRadius = startSphereRadius;
            }

            if (i < patrolPoints.Count - 1)
            {
                endPoint = patrolPoints[i + 1].position;
            }
            else
            {
                endPoint = patrolPoints[0].position;
            }
            
            forward = (endPoint- startPoint).normalized;
            coneOffset = -forward * coneHeight;
            Draw.LineDashed(startPoint, endPoint, lineColor);
            Draw.Cone(endPoint + coneOffset, Quaternion.LookRotation(forward), coneRadius,
                coneHeight, lineColor);
            Draw.Sphere(startPoint, sphereRadius, sphereColor);
            
        }
    }
}
