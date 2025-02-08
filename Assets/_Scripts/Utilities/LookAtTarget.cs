using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class LookAt : MonoBehaviour
{
    public Transform target;
    void Update() {
        transform.LookAt(target);
    }

}