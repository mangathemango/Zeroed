using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitBall : BaseBullet
{
    public float despawnTime = 5;
    public float despawnOnCollisionTime = 0.1f;
    public bool stopAfterCollision = true;

    protected override void OnCollisionEnter(Collision collision) {
        if (IsGameObjectAnEnemy(collision.gameObject, out BaseEnemy enemy)) {
            DealSingleDamage(enemy, damage);
        }
        if (stopAfterCollision) {
            StopBullet();
        }
        Invoke("Destroy", despawnOnCollisionTime);  
    }
}
