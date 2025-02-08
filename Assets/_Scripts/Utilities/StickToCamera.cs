using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashCD : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position + (target.rotation * offset);
        Quaternion perpendicularRotation = Quaternion.LookRotation(target.forward, target.up) * Quaternion.Euler(-90, 0, 0);
        transform.rotation = perpendicularRotation;

        
    }
}
