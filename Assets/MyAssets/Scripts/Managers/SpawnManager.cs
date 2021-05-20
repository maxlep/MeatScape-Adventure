using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : SerializedMonoBehaviour
{
    [SerializeField] private DynamicGameEvent SpawnMeatEvent;
    [SerializeField] private GameObject MeatPickupPrefab;
    [SerializeField] private float MeatPickupYOffset = 3f;
    [SerializeField] private FloatValueReference LuckyCharmStatValue;
    [SerializeField] private GameEvent LuckyCharmActivateEvent;

    private void OnEnable()
    {
        SpawnMeatEvent.Subscribe(SpawnMeatPickup);
    }

    private void OnDisable()
    {
        SpawnMeatEvent.Unsubscribe(SpawnMeatPickup);
    }

    public struct SpawnInfo
    {
        public Vector3 position;
    }

    private void SpawnMeatPickup(System.Object prevSpawnInfoObj, System.Object spawnInfoObj)
    {
        SpawnInfo spawnInfo = (SpawnInfo) spawnInfoObj;
        Vector3 spawnPos = spawnInfo.position + Vector3.up * MeatPickupYOffset;
        GameObject.Instantiate(MeatPickupPrefab, spawnPos, Quaternion.identity);

        //If lucky charm activates, spawn pickup next to this one
        if (TryActivateLuckyCharm())
        {
            LuckyCharmActivateEvent.Raise();
            Vector3 RandomOffset = Random.insideUnitCircle * 3f;
            spawnPos = spawnInfo.position + RandomOffset;
            GameObject.Instantiate(MeatPickupPrefab, spawnPos, Quaternion.identity);
        }
    }

    private bool TryActivateLuckyCharm()
    {
        //Return true if random [0-1] is less than percent to activate lucky charm
        return (Random.value <= LuckyCharmStatValue.Value);
    }
}
