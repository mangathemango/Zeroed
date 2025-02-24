using UnityEngine;

/// <summary>
///* Base enemy class for all enemies in the game
/// TODO: Probably need to generalize this better
/// </summary>
public class BaseEnemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxSpeed = 5;
    [SerializeField] private float deathTime = 1f;
    protected float currentHealth = 100;
    protected float speed;
    protected Rigidbody rb;
    protected Transform playerTransform;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        speed = maxSpeed;
        rb = GetComponent<Rigidbody>();
        playerTransform = GameObject.Find("Player").transform;
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
        
        currentHealth -= damage;
        if (currentHealth <= 0)
        {   
            OnDeath();
        }
        rb.AddForce(knockback, ForceMode.Impulse);
        Stagger(meleeStaggerTime);
    }

    /// <summary>
    ///* Stagger the enemy, which basically means they can't move for a certain amount of time <br/>
    ///! The temp speed is mad sus, and may cause issues in the future <br/>
    /// </summary>
    /// <param name="staggerTime">Stagger time in seconds</param>
    private void Stagger(float staggerTime)
    {
        speed = 0;
        Invoke(nameof(Unstagger), staggerTime);
    }

    /// <summary>
    ///* Unstagger the enemy, which basically means they can move again
    /// </summary>
    private void Unstagger()
    {
        speed = maxSpeed;
    }

    /// <summary>
    /// * What happens when the enemy dies
    /// </summary>
    public virtual void OnDeath() {
        Destroy(gameObject, deathTime);
    }
}
