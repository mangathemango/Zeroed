using NUnit.Framework;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
///* Base bullet class for all bullets in the game
///! This can't work standalone because there's no general damage dealing system in place yet
/// </summary>
public abstract class BaseBullet: MonoBehaviour {
    public float damage = 0f;
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
    }

    protected virtual void OnCollisionEnter(Collision collision) {
        if (IsGameObjectAnEnemy(collision.gameObject, out BaseEnemy enemy)) {
            DealSingleDamage(enemy, damage);
        }
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