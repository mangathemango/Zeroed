using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class LookAt : MonoBehaviour
{
    public Transform target;
    public Vector3 targetPosition;

    void Start () {
        targetPosition = target.position;
    }
    void Update() {
        if (target != null) {
            targetPosition = target.position;
        }
        transform.LookAt(targetPosition);
    }

}