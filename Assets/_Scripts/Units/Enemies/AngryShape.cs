using UnityEngine;

public class AngryShape : BaseEnemy
{
    void Update()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        rb.AddForce(direction * speed);
    }

    public override void OnDeath() {
        base.OnDeath();
        rb.constraints = RigidbodyConstraints.None;
        speed = 0;
    }
}
