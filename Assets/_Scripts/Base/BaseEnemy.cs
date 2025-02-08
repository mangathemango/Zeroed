using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public float health = 100;
    public float deathTime = 1f;
    public float speed = 5;
    private float tempSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage, Vector3 knockback = new Vector3(), float meleeStaggerTime = 0f)
    {   
        
        health -= damage;
        if (health <= 0)
        {   
            Invoke("Die", deathTime);
        }
        GetComponent<Rigidbody>().AddForce(knockback, ForceMode.Impulse);
        if (meleeStaggerTime > 0)
        {
            Stagger(meleeStaggerTime);
        }
    }

    public void Stagger(float staggerTime)
    {
        tempSpeed = speed;
        speed = 0;
        Invoke("Unstagger", staggerTime);
    }

    public void Unstagger()
    {
        speed = tempSpeed;
    }

    public void Die() {
        Destroy(gameObject);
    }
}
