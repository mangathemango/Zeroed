using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{   
    [Header("Movement")]
    public float dashSpeed = 20.0f;
    public float dashCooldownSeconds = 1f;
    public float maxSpeed = 10.0f;
    public int maxDashNum = 2;

    public float currentDashCount = 0;
    private float currentSpeed;
    private Rigidbody rb;
    public float moveHorizontal;
    public float moveVertical;
    Vector3 moveDirection;
    Vector3 dashDirection;

    [Header("Equipment")]
    public GameObject weaponPrefab;
    [System.NonSerialized] public GameObject currentWeapon;

    void Start()
    {   
        currentDashCount = maxDashNum;
        rb = GetComponent<Rigidbody>();
        holdWeapon();
    }

    void holdWeapon() {
        currentWeapon = Instantiate(weaponPrefab, transform.position, transform.rotation, transform);
        BaseGun gun = currentWeapon.GetComponent<BaseGun>();
        if (gun != null) {
            gun.player = transform;
        }
    }

    void Update()
    {
        moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        // rotate momement 45 degrees to the left
        moveDirection = Quaternion.Euler(0, 45, 0) * moveDirection;

        if (moveDirection.magnitude > 0)
        {
            currentSpeed = maxSpeed;
            dashDirection = moveDirection;
        }
        else
        {
            currentSpeed = 0;
        }
        rb.AddForce(moveDirection * currentSpeed);
        if (currentDashCount < maxDashNum) {
            currentDashCount += Time.deltaTime / dashCooldownSeconds;
        } else {
            currentDashCount = maxDashNum;
        }
    }

    public IEnumerator Dash() {
        if (currentDashCount <= 0) {
            yield break;
        }
        currentDashCount--;
        rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);
        yield break;
    }
    public RaycastHit GetPlayerDirection() {
        Ray ray = Camera.main.ScreenPointToRay(Crosshair.Instance.transform.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            return hit;
        } else {
            return new RaycastHit();
        }
    }
}
