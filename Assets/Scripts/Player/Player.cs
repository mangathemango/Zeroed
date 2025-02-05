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

    [Header("References")]
    public Transform DashCDBar;

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

        DashCDBar.localScale = new Vector3(0.1f - (dashTime)/(dashCooldown) * 0.1f,DashCDBar.localScale.y,DashCDBar.localScale.z);

        Vector3 movementX = new Vector3(moveHorizontal, 0.0f, 0.0f);
        Vector3 movementY = new Vector3(0.0f, 0.0f, moveVertical);
        rb.AddForce(movementX * currentSpeed, ForceMode.Force);
        rb.AddForce(movementY * currentSpeed, ForceMode.Force);
    }
}
