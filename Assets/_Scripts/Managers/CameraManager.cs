using System;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> {
    public Transform cameraFollow;
    public Vector3 offset;
    public float smoothTime = 0.3f;

    [NonSerialized] public Vector3 playerOffset = Vector3.zero;
    [NonSerialized] private PlayerMovement playerMovement;
    [NonSerialized] private Transform playerPosition;
    [NonSerialized] private Vector3 playerFollowVelocity = Vector3.zero;
    [NonSerialized] private Vector3 cameraVelocity = Vector3.zero;

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

        cameraFollow.position = Vector3.SmoothDamp(cameraFollow.position, targetPosition, ref playerFollowVelocity, smoothTime);
    
        Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position,cameraFollow.position + offset, ref cameraVelocity, smoothTime);
        Camera.main.transform.LookAt(cameraFollow);
    }

    public void RotateCounterclockwise(float degrees) {
        offset = Quaternion.Euler(0, -degrees, 0) * offset;
    }
    public void RotateClockwise(float degrees) {
        offset = Quaternion.Euler(0, degrees, 0) * offset;
    }
}