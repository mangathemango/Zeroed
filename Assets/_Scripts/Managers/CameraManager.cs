using System;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    public Transform cameraFollow;
    public Vector3 offset;
    public float smoothTime = 0.3f;

    [NonSerialized] public Vector3 playerOffset = Vector3.zero;
    [NonSerialized] private PlayerMovement playerMovement;
    [NonSerialized] private Transform playerPosition;
    [NonSerialized] private Vector3 velocity = Vector3.zero;

    void Start () {
        if (!cameraFollow) {
            Debug.LogError("Camera follow is not set!");
        }
        GameObject player = GameObject.Find("Player");
        playerPosition = player.transform;
        playerMovement = player.GetComponent<PlayerMovement>();
        cameraFollow.position = playerPosition.position;
    }
    void Update() {
        Vector3 targetPosition = playerPosition.position + playerMovement.ConvertToPlayerDirection(playerOffset);

        cameraFollow.position = Vector3.SmoothDamp(cameraFollow.position, targetPosition, ref velocity, smoothTime);
    
        transform.position = cameraFollow.position + offset;
        transform.LookAt(cameraFollow);
    }
}