using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCursor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Get mouse position in 3d as Ray
        Ray ray = Camera.main.ScreenPointToRay(Crosshair.Instance.placement.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {   
            Vector3 targetPosition = hit.point;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }

    }
}
