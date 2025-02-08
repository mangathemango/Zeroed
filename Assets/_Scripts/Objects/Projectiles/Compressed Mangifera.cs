using UnityEngine;

class CompressedMangifera: BaseBullet {
    public float despawnTime = 5;
    public float despawnOnCollisionTime = 0.1f;
    public float explosionRadius = 5f;
    public float explosionForce = 5f;
    public bool stopAfterCollision = true;
    private bool firstHit = true;
    void Start() {
        SetupBullet();
        Invoke("Destroy", despawnTime);
    }

    void Update() {
        if (hit && firstHit) {
            if (stopAfterCollision) {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().mass = 0;
                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<SphereCollider>().enabled = false;
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var hitCollider in hitColliders) {
                if (hitCollider.GetComponent<BaseEnemy>() != null) {
                    float damage = 10f;
                    hitCollider.GetComponent<BaseEnemy>().TakeDamage(damage , -explosionForce * (transform.position - hitCollider.transform.position).normalized);
                    hitCollider.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
            }
            Debug.Log("Hit object: " + hitObject.name);
            Invoke("Destroy", despawnOnCollisionTime);  
            firstHit = false;
        }
    }

}