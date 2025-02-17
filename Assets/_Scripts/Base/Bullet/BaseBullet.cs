using NUnit.Framework;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
///* Base bullet class for all bullets in the game<br/>
///* Contains the basic properties of a bullet, such as damage, despawn time, etc.<br/><br/>
///
///? BaseBullet doesn't deal damage to enemies, but it provides the basic properties of a bullet, such as despawn time.<br/>
///? Damage dealing is done by the child classes of BaseBullet, such as SingleBullet and ExplosionBullet.<br/>
/// </summary>
public class BaseBullet: MonoBehaviour {
    public float damage = 0f;
    public float despawnTime = 5;
    public float despawnOnCollisionTime = 0.1f;
    public bool stopAfterCollision = true;
    protected Rigidbody rb;
    protected SphereCollider sc;



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
    /// * Check if a game object is an enemy, and return the enemy if it is
    /// </summary>
    /// <param name="go">The input game object</param>
    /// <param name="enemy">The BaseEnemy component of the game object</param>
    /// <returns>Whether the game object is an enemy</returns>
    protected bool IsGameObjectAnEnemy(GameObject go, out BaseEnemy enemy) {
        enemy = go.GetComponent<BaseEnemy>();
        return enemy != null;
    }

    /// <summary>
    /// * Deal damage to a single enemy <br/><br/>
    /// ? This is used by the SingleBullet class to deal damage to a single enemy <br/>
    /// </summary>
    /// <param name="enemy">The BaseEnemy component of a game object</param>
    /// <param name="damage">The amount of damage to deal</param>
    protected void DealSingleDamage(BaseEnemy enemy, float damage) {
        enemy.TakeDamage(damage);
    }

    /// <summary>
    /// * Deal damage to all enemies within a certain radius <br/><br/>
    /// ? This is used by the ExplosionBullet class to deal damage to all enemies within a certain radius <br/>
    /// </summary>
    /// <param name="explosionForce">How much the enemy is knocked back away from the bullet</param>
    /// <param name="explosionRadius">The AOE radius of the explosion</param>
    protected void DealExplosionDamage(float explosionForce, float explosionRadius) {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders) {
            if (IsGameObjectAnEnemy(hitCollider.gameObject, out BaseEnemy enemy)) {
                Vector3 explosionDirection = (transform.position - hitCollider.transform.position).normalized;
                enemy.TakeDamage(damage , -explosionForce * explosionDirection);
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