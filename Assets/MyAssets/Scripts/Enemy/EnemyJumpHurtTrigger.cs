using UnityEngine;

public class EnemyJumpHurtTrigger : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;
    
    public void DamageEnemy(int dmg)
    {
        enemyController.DamageEnemy(dmg);
    }
}
