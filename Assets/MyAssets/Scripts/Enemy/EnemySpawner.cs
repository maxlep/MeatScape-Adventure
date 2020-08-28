using UnityEngine;
using Sirenix.OdinInspector;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyController enemyPrefab;
    [SuffixLabel("seconds", Overlay = true)] [SerializeField] private float spawnTime = 300;

    private void Awake() {
        SpawnEnemy();
    }

    private void SpawnEnemy() {
        Instantiate(enemyPrefab, this.transform).OnDeath += () => {
            LeanTween.value(0f, 1f, spawnTime).setOnComplete(_ => SpawnEnemy());
        };
    }
}
