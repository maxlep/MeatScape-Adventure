using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

/// <summary>
/// Singleton class
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;
    
    [Serializable]
    public class PoolObject
    {
        public GameObject poolObjectPrefab;
        public int poolSize;
    }

    [SerializeField] private List<PoolObject> poolObjects;

    private Dictionary<GameObject, PoolObject> poolMapping;
    private Dictionary<GameObject, Dictionary<int, GameObject>> pools;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        poolMapping = new Dictionary<GameObject, PoolObject>();
        foreach(PoolObject po in poolObjects)
        {
            poolMapping.Add(po.poolObjectPrefab, po);
        }
        pools = new Dictionary<GameObject, Dictionary<int, GameObject>>();
        foreach(PoolObject po in poolObjects)
        {
           SetupPool(po);
        }
    }

    private void SetupPool(PoolObject po)
    {
        pools.Add(po.poolObjectPrefab, new Dictionary<int, GameObject>());
        var objectPool = pools[po.poolObjectPrefab];

        GameObject newObj = null;
        for (int i = 0; i < po.poolSize; i++)
        {
            newObj = Instantiate(po.poolObjectPrefab, transform);
            newObj.SetActive(false);
            objectPool.Add(newObj.GetInstanceID(), newObj);
        }
    }

    public GameObject GetObjectFromPool(GameObject poolObjectPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolMapping.ContainsKey(poolObjectPrefab)) return null;

        var objectFromPool = pools[poolObjectPrefab].Values.FirstOrDefault(obj => !obj.activeSelf);
        if (objectFromPool == null)
        {
            objectFromPool = Instantiate(poolMapping[poolObjectPrefab].poolObjectPrefab, transform);
            pools[poolObjectPrefab].Add(objectFromPool.GetInstanceID(), objectFromPool);
            // Debug.Log($"Had to increase pool size for: {poolObjectPrefab.name} to {pools[poolObjectPrefab].Count}");
        }

        objectFromPool.transform.position = position;
        objectFromPool.transform.rotation = rotation;
        objectFromPool.transform.parent = parent;
        objectFromPool.SetActive(true);
        
        return objectFromPool;
    }

    public bool ReturnObjectToPool(GameObject poolObjectPrefab, GameObject gameObject)
    {
        if (!poolMapping.ContainsKey(poolObjectPrefab)) return false;
        var objectPool = pools[poolObjectPrefab];

        var existsInPool = objectPool.TryGetValue(gameObject.GetInstanceID(), out var objectToReturn);
        if(!existsInPool) return false;
        
        if (objectPool.Count > poolMapping[poolObjectPrefab].poolSize)
        {
            objectPool.Remove(objectToReturn.GetInstanceID());
            // Debug.Log($"Decreased pool size for: {poolObjectPrefab.name} to {pools[poolObjectPrefab].Count}");
            Destroy(objectToReturn);
            return true;
        }
        
        objectToReturn.SetActive(false);
        objectToReturn.transform.parent = transform;
        return true;
    }

    public void ReturnObjectToPoolDelayed(GameObject poolObjectPrefab, GameObject gameObject, float delay, Action<bool> onComplete = null)
    {
        LeanTween.value(0, 1, delay).setOnComplete(() => {
            bool returned = this.ReturnObjectToPool(poolObjectPrefab, gameObject);
            if(onComplete != null) onComplete(returned);
        });
    }

    public void CreatePool(GameObject poolObjectPrefab, int poolSize)
    {
        if (poolMapping.ContainsKey(poolObjectPrefab)) return;

        PoolObject po = new PoolObject{ poolObjectPrefab = poolObjectPrefab, poolSize = poolSize };
        poolMapping.Add(poolObjectPrefab, po);
        SetupPool(po);
    }

    public void RemovePool(GameObject poolObjectPrefab)
    {
        if (!poolMapping.ContainsKey(poolObjectPrefab)) return;

        poolMapping.Remove(poolObjectPrefab);
        ClearPool(poolObjectPrefab);
        pools.Remove(poolObjectPrefab);
    }

    public void ClearPool(GameObject poolObjectPrefab)
    {
        if (!poolMapping.ContainsKey(poolObjectPrefab)) return;

        foreach(GameObject obj in pools[poolObjectPrefab].Values)
        {
            Destroy(obj);
        }
        pools[poolObjectPrefab].Clear();
    }
}
