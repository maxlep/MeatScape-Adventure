using UnityEngine;

public class EnemyHurtProxy : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;

    public EnemyController EnemyController => enemyController;


    public virtual void DamageEnemy(int dmg)
    {
        enemyController.DamageEnemy(dmg, Vector3.zero, false);
    }

    public virtual void DamageEnemy(int dmg, Vector3 knockBackDirection, bool applyKnockBack = true, float knockForce = 50f) {
        enemyController.DamageEnemy(dmg, knockBackDirection, applyKnockBack, knockForce);
    }
}
