using System.Collections;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float dashSpeed = 20.0f;
    [SerializeField] private float dashCooldownSeconds = 1f;
    [SerializeField] private float maxSpeed = 10.0f;
    [SerializeField] private int maxDashNum = 2;

    private float currentDashCount = 0;
    public float getCurrentDashCount {
        get {return currentDashCount;} 
    }
    public float moveHorizontal {get; set;}
    public float moveVertical {get; set;}

    private float currentSpeed;
    private Vector3 moveDirection;
    private Vector3 dashDirection;
    private Rigidbody rb;

    void Start()
    {
        currentDashCount = maxDashNum;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {   
        UpdateDashCount();
        UpdateSpeed();
        Move();
    }

    void UpdateDashCount() {
        if (currentDashCount < maxDashNum)
        {
            currentDashCount += Time.deltaTime / dashCooldownSeconds;
        }
        else
        {
            currentDashCount = maxDashNum;
        }
    }
    
    void UpdateSpeed() {
        if (moveDirection.magnitude > 0)
        {
            currentSpeed = maxSpeed;
            dashDirection = moveDirection;
        }
        else
        {
            currentSpeed = 0;
        }
    }

    void Move() {
        moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        moveDirection = CameraManager.Instance.RotateToCameraForwardDirection(moveDirection);
        rb.AddForce(moveDirection * currentSpeed);
    }

    public void Dash()
    {
        if (currentDashCount <= 0)
        {
            return;
        }
        currentDashCount--;
        rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);
    }
}
