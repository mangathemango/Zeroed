using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;   
    public float smoothTime = 0.3f; 

    private Vector3 velocity = Vector3.zero;
    public Vector3 targetPosition;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 targetPosition = target.position + offset;
        transform.position = targetPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null) {
            targetPosition = target.position + offset;
            transform.position = targetPosition;
            return;
        }
        transform.position = targetPosition;
    }
}
