using UnityEngine;

/// <summary>
/// * Base class for bullets that deal damage to a single enemy<br/>
/// </summary>
public class SingleBullet : BaseBullet
{
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (IsGameObjectAnEnemy(collision.gameObject, out BaseEnemy enemy))
        {
            DealSingleDamage(enemy, damage);
        }
    }
}