using UnityEngine;

/// <summary>
///* Base enemy class for all enemies in the game
/// TODO: Probably need to generalize this better
/// </summary>
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
    /// <summary>
    ///* Take damage from a bullet <br/>
    ///! This is just a temporary implementation, and will be replaced with a more general damage dealing system
    /// </summary>
    /// <param name="damage">How much damage is dealt</param>
    /// <param name="knockback">The knockback force</param>
    /// <param name="meleeStaggerTime">Stagger time in seconds</param>
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
    /// <summary>
    ///* Stagger the enemy, which basically means they can't move for a certain amount of time <br/>
    ///! The temp speed is mad sus, and may cause issues in the future <br/>
    ///TODO: Probably separate baseSpeed and currentSpeed apart from each other
    /// </summary>
    /// <param name="staggerTime">Stagger time in seconds</param>
    public void Stagger(float staggerTime)
    {
        tempSpeed = speed;
        speed = 0;
        Invoke("Unstagger", staggerTime);
    }

    /// <summary>
    ///* Unstagger the enemy, which basically means they can move again
    /// </summary>
    public void Unstagger()
    {
        speed = tempSpeed;
    }

    public void Die() {
        Destroy(gameObject);
    }
}
