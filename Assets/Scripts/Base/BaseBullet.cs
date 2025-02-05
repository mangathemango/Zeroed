using Unity.VisualScripting;
using UnityEngine;

public class BaseBullet: MonoBehaviour {
    public bool hit = false;
    public GameObject hitObject;
    public GameObject source;
    void OnCollisionEnter(Collision collision) {
        hit = true;
        hitObject = collision.gameObject;
    }

    public void Destroy() {
        Destroy(gameObject);
    }
}