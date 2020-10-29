using System;
using System.Collections.Generic;
using AmplifyShaderEditor;
using Shapes;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Jobs;

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
        Draw.Sphere(ShapesBlendMode.Transparent, ThicknessSpace.Meters, transform.position, .5f, new Color(1f, 1f, 0f, .35f));
    }
}
