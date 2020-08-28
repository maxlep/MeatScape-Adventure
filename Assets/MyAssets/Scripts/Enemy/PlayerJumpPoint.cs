using UnityEngine;

public class PlayerJumpPoint : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;
    [SerializeField] private new Collider collider;

    private void OnTriggerEnter(Collider other) {
        GameObject otherGameObject = other.gameObject;
        if(otherGameObject.layer == LayerMask.NameToLayer("Player") && otherGameObject.tag == "Player") {
            CapsuleCollider playerCollider = (CapsuleCollider)other;
            float playerBottom = playerCollider.bounds.center.y - playerCollider.height / 4;
            if(playerBottom > collider.bounds.center.y) {
                enemyController.DamageEnemy(1);
            }
        }
    }
}
