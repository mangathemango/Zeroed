using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitBall : BaseBullet
{
    public float despawnTime = 5;
    public float despawnOnCollisionTime = 0.1f;
    public bool stopAfterCollision = true;
    private bool firstHit = true;
    // Start is called before the first frame update
    void Start()
    {
        SetupBullet();
        Invoke("Destroy", despawnTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (hit && firstHit) {
            if (stopAfterCollision) {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().mass = 0;
                GetComponent<Rigidbody>().useGravity = false;
                GetComponent<SphereCollider>().enabled = false;
            }

            if (hitObject.GetComponent<BaseEnemy>() != null) {
                float damage = 10f;
                hitObject.GetComponent<BaseEnemy>().TakeDamage(damage);
            }
            Debug.Log("Hit object: " + hitObject.name);
            Invoke("Destroy", despawnOnCollisionTime);  
            firstHit = false;
        }
    }
}
