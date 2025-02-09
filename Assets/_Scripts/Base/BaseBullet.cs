using Unity.VisualScripting;
using UnityEngine;

public class BaseBullet: MonoBehaviour {
    public bool hit = false;
    public GameObject hitObject;
    public GameObject source;
    public void SetupBullet() {
        if (!source) {
            Debug.LogError("Source not found for bullet");
        }
        if (!source.GetComponent<BaseGun>()) {
            Debug.LogError("Source does not have BaseGun component");
        }
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
    protected virtual void Start() {
        SetupBullet();
    }
}