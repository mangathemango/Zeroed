using UnityEngine;

class CompressedMangifera: BaseBullet {
    public float despawnTime = 5;
    public float despawnOnCollisionTime = 0.1f;
    public float explosionRadius = 5f;
    public float explosionForce = 5f;
    public bool stopAfterCollision = true;
    private bool firstHit = true;


    protected override void OnCollisionEnter(Collision collision) {
        if (stopAfterCollision) {
            StopBullet();
        }
        DealExplosionDamage(explosionForce, explosionRadius);
        Invoke("Destroy", despawnOnCollisionTime);  
    }
}