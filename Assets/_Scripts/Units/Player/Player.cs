using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{   
    [Header("Movement")]
    public float dashSpeed = 20.0f;
    public float dashCooldown = 1f;
    public float maxSpeed = 10.0f;

    private float currentSpeed;
    private float dashTime = 0;
    private Rigidbody rb;
    private float moveHorizontal;
    private float moveVertical;
    private Vector3 dashMovement;

    [Header("Equipment")]
    public GameObject weapon;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        holdWeapon();
    }

    void holdWeapon() {
        Instantiate(weapon, transform.position, transform.rotation, transform);
        BaseGun gun = weapon.GetComponent<BaseGun>();
        if (gun != null) {
            gun.Player = transform;
        }
    }

    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        // rotate momement 45 degrees to the left
        movement = Quaternion.Euler(0, 45, 0) * movement;

        if (movement.magnitude > 0)
        {
            currentSpeed = maxSpeed;
            dashMovement = movement;
        }
        else
        {
            currentSpeed = 0;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashTime <= 0)
        {
            dashTime = dashCooldown;
            rb.AddForce(dashMovement * dashSpeed, ForceMode.Impulse);
        }
        if (dashTime > 0)
        {
            dashTime -= Time.deltaTime;
        }

        rb.AddForce(movement * currentSpeed);
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
