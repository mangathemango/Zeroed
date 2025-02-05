using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Transform target;

    public Vector3 offsetPosition;
    public Vector3 offsetRotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the camera around the target based on its own rotation
        transform.RotateAround(target.position, Vector3.up, transform.rotation.y);
        transform.position = target.position - (transform.rotation * offsetPosition);
        transform.Rotate(offsetRotation);
    }
}
