using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlerHill : MonoBehaviour
{
    [SerializeField] private Transform DroneSpawnerTransform;
    [SerializeField] private EnemySpawner QueenSpawner;

    private EnemySpawner[] droneSpawners;

    void Awake() {
        droneSpawners = DroneSpawnerTransform.GetComponentsInChildren<EnemySpawner>();
    }

    public void StopDroneSpawning() {
        foreach(var droneSpawner in droneSpawners) {
            droneSpawner.StopSpawning();
        }
    }
}
