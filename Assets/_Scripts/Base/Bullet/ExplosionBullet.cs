using UnityEngine;

/// <summary>
/// * Base class for bullets that explode on impact, dealing damage to all enemies within a certain radius<br/>
/// </summary>
class ExplosionBullet: BaseBullet {
    public float explosionRadius = 5f;
    public float explosionForce = 5f;
    protected override void OnCollisionEnter(Collision collision) {
        base.OnCollisionEnter(collision);
        DealExplosionDamage(explosionForce, explosionRadius);
    }
}