using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : BaseEnemy
{
    public Transform player;
    private Rigidbody rb;
    // Update is called once per frame
    void Start() {
        if (player == null)
        {
            GameObject playerGameObject = GameObject.Find("Player");
            if (playerGameObject != null)
            {
                player = playerGameObject.transform;
            }
        }
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        rb.AddForce(direction * speed);
        if (health <= 0)
        {
            rb.constraints = RigidbodyConstraints.None;
            speed = 0;
        }
    }
}
