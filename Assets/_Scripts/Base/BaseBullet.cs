using UnityEngine;

/// <summary>
///* Base bullet class for all bullets in the game
///! This can't work standalone because there's no general damage dealing system in place yet
/// </summary>
public class BaseBullet: MonoBehaviour {
    public bool hit = false;
    public GameObject hitObject;
    public float damage = 0f;

    /// <summary>
    ///* Setup the bullet with a rigidbody if it doesn't have one
    /// TODO: Check for more bullet properties, such as colliders, etc.
    /// </summary>
    public void SetupBullet() {
        if (!GetComponent<Rigidbody>()) {
            gameObject.AddComponent<Rigidbody>();
        }
    }
    void OnCollisionEnter(Collision collision) {
        hit = true;
        hitObject = collision.gameObject;
    }

    public void Destroy() {
        Destroy(gameObject);
    }
}