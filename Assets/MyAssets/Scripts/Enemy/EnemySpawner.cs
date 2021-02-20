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
    
    [SerializeField] [ValueDropdown("GetEnemyDropdown")] [OnValueChanged("StoreEnemyPrefabMeshes")]
    private GameObject EnemyPrefab;
    
    [SerializeField] [PropertySpace(5f, 5f)]
    private EnemyTypeContainer EnemyTypes;
    
    private List<(Transform transform, Mesh mesh)> enemyTransformMeshTuples = new List<(Transform transform, Mesh mesh)>();

    private PatrolPointHelper patrolPointHelper;
    

    #region Inspector/Odin Methods

    private List<GameObject> GetEnemyDropdown()
    {
        return EnemyTypes.EnemyPrefabs;
    }

    #endregion

    
    private void Awake()
    {
        SpawnEnemy();

        if (patrolPointHelper == null)
            patrolPointHelper = GetComponent<PatrolPointHelper>();
    }

    private void SpawnEnemy() {
        GameObject enemy = Instantiate(EnemyPrefab, transform.position, transform.rotation, transform);
        EnemyController enemyScript = enemy.GetComponentInChildren<EnemyController>();
        
        //If helper script on same object, send patrol points to enemy
        if (patrolPointHelper != null)
            enemyScript.Initialize(patrolPointHelper.PatrolPoints);

        //Dont respawn if spawnTime = 0f
        if (!Mathf.Approximately(SpawnTime, 0f))
        {
            enemyScript.OnDeath += () => {
                TimeUtils.SetTimeout(SpawnTime, () => SpawnEnemy());
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
