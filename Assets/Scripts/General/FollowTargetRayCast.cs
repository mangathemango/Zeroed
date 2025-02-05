using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetRayCast : MonoBehaviour
{
    public Transform target;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Ray ray = new Ray(target.position, target.forward);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            transform.position = raycastHit.point;
        }
    }
}
