using UnityEngine;

public class SingleBullet : BaseBullet
{
    protected override void OnCollisionEnter(Collision collision)
    {
        if (IsGameObjectAnEnemy(collision.gameObject, out BaseEnemy enemy))
        {
            DealSingleDamage(enemy, damage);
        }
    }
}