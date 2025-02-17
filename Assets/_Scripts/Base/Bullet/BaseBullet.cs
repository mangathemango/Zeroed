using NUnit.Framework;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
///* Base bullet class for all bullets in the game<br/>
///* Contains the basic properties of a bullet, such as damage, despawn time, etc.<br/><br/>
///
///? Currently, this still can't work standalone, as it needs to be inherited by a bullet class like ExplosionBullet or SingleBullet<br/>
/// </summary>
public class BaseBullet: MonoBehaviour {
    public float damage = 0f;
    public float despawnTime = 5;
    public float despawnOnCollisionTime = 0.1f;
    public bool stopAfterCollision = true;
    protected Rigidbody rb;
    protected SphereCollider sc;



    /// <summary>
    ///* Setup the bullet with a rigidbody if it doesn't have one
    /// TODO: Check for more bullet properties, such as colliders, etc.
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

        Invoke("Destroy", despawnTime);
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if (stopAfterCollision) {
            StopBullet();
        }
        Invoke("Destroy", despawnOnCollisionTime);  
    }
    
    protected bool IsGameObjectAnEnemy(GameObject go, out BaseEnemy enemy) {
        enemy = go.GetComponent<BaseEnemy>();
        return enemy != null;
    }
    protected void DealSingleDamage(BaseEnemy enemy, float damage) {
        enemy.TakeDamage(damage);
    }

    protected void DealExplosionDamage(float explosionForce, float explosionRadius) {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders) {
            if (IsGameObjectAnEnemy(hitCollider.gameObject, out BaseEnemy enemy)) {
                Vector3 explosionDirection = (transform.position - hitCollider.transform.position).normalized;
                enemy.TakeDamage(damage , -explosionForce * explosionDirection);
            }
        }
    }
    protected void StopBullet() {
        rb.linearVelocity = Vector3.zero;
        rb.mass = 0;
        rb.useGravity = false;
        sc.enabled = false;
    }
    protected void Destroy() {
        Destroy(gameObject);
    }
}