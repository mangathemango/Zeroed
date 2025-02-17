using UnityEngine;

class ExplosionBullet: BaseBullet {
    public float explosionRadius = 5f;
    public float explosionForce = 5f;
    protected override void OnCollisionEnter(Collision collision) {
        DealExplosionDamage(explosionForce, explosionRadius);
    }
}