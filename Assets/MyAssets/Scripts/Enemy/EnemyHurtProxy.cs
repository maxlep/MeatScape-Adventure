using UnityEngine;

public class EnemyHurtProxy : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;

    public EnemyController EnemyController => enemyController;
}
