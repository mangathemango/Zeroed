using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{   
    [Header("Movement")]
    public float dashSpeed = 20.0f;
    public float dashCooldownSeconds = 1f;
    public float maxSpeed = 10.0f;
    public int maxDashNum = 2;

    [NonSerialized] public float currentDashCount = 0;
    [NonSerialized] public float moveHorizontal;
    [NonSerialized] public float moveVertical;
    private float currentSpeed;
    private Vector3 moveDirection;
    private Vector3 dashDirection;

    [Header("References")]

    [NonSerialized] public GameObject currentWeapon;
    private Rigidbody rb;

    void Start()
    {   
        currentDashCount = maxDashNum;
        rb = GetComponent<Rigidbody>();
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
        if (currentDashCount < maxDashNum) {
            currentDashCount += Time.deltaTime / dashCooldownSeconds;
        } else {
            currentDashCount = maxDashNum;
        }
        rb.AddForce(moveDirection * currentSpeed);
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
