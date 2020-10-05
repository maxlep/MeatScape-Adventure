using KinematicCharacterController;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class EnemyController : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor characterMotor;
    [SerializeField] private LayerMapper layerMapper;
    [SerializeField, SuffixLabel("m/s^2", Overlay = true)] private float Gravity = 10f;
    [SerializeField] private int MaxHealth = 1;
    [SerializeField] private GameObject deathParticles;

    public delegate void OnDeath_();
    public event OnDeath_ OnDeath;

    private int health;

    #region Unity Methods

    private void Awake() {
        characterMotor.CharacterController = this;
        health = MaxHealth;
    }

    private void LateUpdate() {
        if(health <= 0) {
            KillEnemy();
        }
    }

    #endregion

    #region CharacterController Methods

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        if(hitCollider.gameObject.layer == layerMapper.GetLayer(LayerEnum.PlayerProjectile)) {
            health -= 1;
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //Gravity
        if (!characterMotor.GroundingStatus.IsStableOnGround)
            currentVelocity.y = currentVelocity.y - (Gravity * Time.deltaTime);
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {

    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }

    #endregion

    #region Death Methods

    private void KillEnemy() {
        OnDeath?.Invoke();
        KinematicCharacterSystem.UnregisterCharacterMotor(characterMotor);
        EffectsManager.Instance?.SpawnParticlesAtPoint(deathParticles, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void DamageEnemy(int dmg) {
        health -= dmg;
    }

    #endregion
}
