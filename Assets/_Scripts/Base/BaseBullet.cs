using Unity.VisualScripting;
using UnityEngine;

public class BaseBullet: MonoBehaviour {
    public bool hit = false;
    public GameObject hitObject;
    public float damage = 0f;
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