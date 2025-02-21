using System.Collections;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float dashSpeed = 20.0f;
    public float dashCooldownSeconds = 1f;
    public float maxSpeed = 10.0f;
    public int maxDashNum = 2;

    private float currentDashCount = 0;
    public float getCurrentDashCount {
        get {return currentDashCount;} 
    }
    public float moveHorizontal {get; set;}
    public float moveVertical {get; set;}

    [NonSerialized] private float currentSpeed;
    [NonSerialized] private Vector3 moveDirection;
    [NonSerialized] private Vector3 dashDirection;
    [NonSerialized] private Vector3 forwardDirection;

    [Header("References")]
    [NonSerialized] public GameObject currentWeapon;
    [NonSerialized] private Rigidbody rb;

    void Start()
    {
        currentDashCount = maxDashNum;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {   
        forwardDirection = Camera.main.transform.forward;
        forwardDirection.y = 0;
        moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        moveDirection = Quaternion.LookRotation(forwardDirection) * moveDirection;
        

        if (moveDirection.magnitude > 0)
        {
            currentSpeed = maxSpeed;
            dashDirection = moveDirection;
        }
        else
        {
            currentSpeed = 0;
        }
        if (currentDashCount < maxDashNum)
        {
            currentDashCount += Time.deltaTime / dashCooldownSeconds;
        }
        else
        {
            currentDashCount = maxDashNum;
        }
        rb.AddForce(moveDirection * currentSpeed);
    }

    public IEnumerator Dash()
    {
        if (currentDashCount <= 0)
        {
            yield break;
        }
        currentDashCount--;
        rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);
        yield break;
    }

    public Vector3 ConvertToPlayerDirection(Vector3 direction)
    {
        return Quaternion.LookRotation(forwardDirection) * direction;
    }
}
