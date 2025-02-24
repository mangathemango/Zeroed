using UnityEngine;

/// <summary>
///* Base bullet class for all bullets in the game<br/>
///* Contains the basic properties of a bullet, such as damage, despawn time, etc.<br/><br/>
///
///? BaseBullet doesn't deal damage to enemies, but it provides the basic properties of a bullet, such as despawn time.<br/>
///? Damage dealing is done by the child classes of BaseBullet, such as SingleBullet and ExplosionBullet.<br/>
/// </summary>
public class BaseBullet: MonoBehaviour {
    [SerializeField] private float _damage = 0f;
    [SerializeField] private float despawnTime = 5;
    [SerializeField] private float despawnOnCollisionTime = 0.1f;
    [SerializeField] private bool stopAfterCollision = true;
    protected Rigidbody rb;
    protected SphereCollider sc;

    /// <summary>
    /// * Set the damage of the bullet. <br/><br/>
    /// 
    /// ? This function is currently used by BaseGun to set the damage based on the gun's damage stats <br/>
    /// </summary>
    /// <param name="damage">The amount of damage to deal</param>
    public void SetDamage(float damage) {
        _damage = damage;
    }

    /// <summary>
    ///* Setup the bullet with a rigidbody if it doesn't have one <br/><br/>
    /// TODO: Check for more bullet properties, such as colliders, etc. <br/>
    /// </summary>
    protected void Start() {
        rb = GetComponent<Rigidbody>();
        if (!rb) {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        sc = GetComponent<SphereCollider>();
        if (!sc) {
            sc = gameObject.AddComponent<SphereCollider>();
        }

        Destroy(gameObject, despawnTime);
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if (stopAfterCollision) {
            StopBullet();
        }
        Destroy(gameObject, despawnOnCollisionTime);
    }

    /// <summary>
    /// * Deal damage to a single enemy <br/><br/>
    /// ? This is used by the SingleBullet class to deal damage to a single enemy <br/>
    /// </summary>
    /// <param name="enemy">The BaseEnemy component of a game object</param>
    /// <param name="damage">The amount of damage to deal</param>
    protected void DealSingleDamage(BaseEnemy enemy) {
        enemy.TakeDamage(_damage);
    }

    /// <summary>
    /// * Deal damage to all enemies within a certain radius <br/><br/>
    /// ? This is used by the ExplosionBullet class to deal damage to all enemies within a certain radius <br/>
    /// </summary>
    /// <param name="explosionForce">How much the enemy is knocked back away from the bullet</param>
    /// <param name="explosionRadius">The AOE radius of the explosion</param>
    protected void DealExplosionDamage(float explosionForce, float explosionRadius) {
        Collider[] hitColliders = new Collider[100];
        Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, hitColliders);
        foreach (var hitCollider in hitColliders) {
            if (hitCollider.gameObject.TryGetComponent(out BaseEnemy enemy)) {
                Vector3 explosionDirection = (transform.position - hitCollider.transform.position).normalized;
                enemy.TakeDamage(_damage , -explosionForce * explosionDirection);
            }
        }
    }

    /// <summary>
    /// * Stop the bullet from moving <br/><br/>
    /// ? Right now, every bullet stops moving when it hits something, but this can be changed in the future if we want bouncing bullets<br/>
    /// </summary>
    protected void StopBullet() {
        rb.linearVelocity = Vector3.zero;
        rb.mass = 0;
        rb.useGravity = false;
        sc.enabled = false;
    }
}