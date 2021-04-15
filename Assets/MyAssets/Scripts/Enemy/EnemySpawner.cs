using System.Collections.Generic;
using Shapes;
using UnityEngine;
using Sirenix.OdinInspector;
using MyAssets.Scripts.Utils;
using Sirenix.Utilities;

public class EnemySpawner : MonoBehaviour
{
    [SuffixLabel("seconds", Overlay = true)] [SerializeField] 
    private float SpawnTime = 1;

    [SerializeField] private bool SpawnOnAwake;

    [SerializeField] private int Count = 1;
    [SerializeField] private int IncrementCountOnDeath = 0;
    
    [SerializeField] [ValueDropdown("GetEnemyDropdown")] [OnValueChanged("StoreEnemyPrefabMeshes")]
    private GameObject EnemyPrefab;
    
    [SerializeField] [PropertySpace(5f, 5f)]
    private EnemyTypeContainer EnemyTypes;
    
    private List<(Transform transform, Mesh mesh)> enemyTransformMeshTuples = new List<(Transform transform, Mesh mesh)>();

    private PatrolPointHelper patrolPointHelper;

    private List<EnemyController> _spawnedEnemeies = new List<EnemyController>();
    private object _spawnedEnemiesLock = new object();

    #region Inspector/Odin Methods

    private List<GameObject> GetEnemyDropdown()
    {
        return EnemyTypes.EnemyPrefabs;
    }

    #endregion

    private void Awake()
    {
        if (patrolPointHelper == null)
            patrolPointHelper = GetComponent<PatrolPointHelper>();

        if (SpawnOnAwake)
        {
            TrySpawnEnemy();
        }
        else
        {
            TimeUtils.SetTimeout(SpawnTime, () => TrySpawnEnemy());
        }
    }

    private bool TrySpawnEnemy()
    {
        bool doSpawn = false;
        lock (_spawnedEnemiesLock)
        {
            if (_spawnedEnemeies.Count < Count)
            {
                doSpawn = true;
            }
        }

        if (doSpawn)
        {
            SpawnEnemy();
            TimeUtils.SetTimeout(SpawnTime, () => TrySpawnEnemy());
            return true;
        }

        return false;
    }

    private void SpawnEnemy() {
        GameObject enemy = Instantiate(EnemyPrefab, transform.position, transform.rotation, transform);
        EnemyController enemyScript = enemy.GetComponentInChildren<EnemyController>();
        lock (_spawnedEnemiesLock)
        {
            _spawnedEnemeies.Add(enemyScript);
        }
        
        //If helper script on same object, send patrol points to enemy
        if (patrolPointHelper != null)
        {
            enemyScript.Initialize(patrolPointHelper.PatrolPoints);
        }

        //Dont respawn if spawnTime = 0f
        if (!Mathf.Approximately(SpawnTime, 0f))
        {
            enemyScript.OnDeath += () => {
                TimeUtils.SetTimeout(SpawnTime, () => TrySpawnEnemy());
                lock (_spawnedEnemiesLock)
                {
                    Count += IncrementCountOnDeath;
                    _spawnedEnemeies.Remove(enemyScript);
                }
            };
        }
    }
    
    private void OnValidate()
    {
        if (enemyTransformMeshTuples.IsNullOrEmpty())
        {
            StoreEnemyPrefabMeshes();
        }
    }

    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
        DrawEnemyMeshGizmo();
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
