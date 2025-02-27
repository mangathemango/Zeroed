using System;
using UnityEngine;

/// <summary>
/// * Manages the camera's movements, rotations, and so on<br/><br/>
/// ? The core concept of the camera is simple: it follows and looks at the player.<br/>
/// ? However, if the camera is set to look directly at the player's position, it doesn't look really good.<br/>
/// ? Because the camera's movement is smooth, the camera angle changes every time the player moves, which is really trippy. <br/>
/// ? That's why an invisible object called [cameraFollow] is used instead.<br/>
/// ? The cameraFollow object follows the player's position with a slight offset, and the camera looks at the cameraFollow object.<br/>
/// ? This way, the camera's movement is smooth, and the player's movement doesn't affect the camera's angle as much.<br/>
/// </summary>
public class CameraManager : Singleton<CameraManager> {
    [SerializeField] private Transform cameraFollow;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector3 basePlayerOffset;
    private Vector3 playerFollowVelocity = Vector3.zero;
    private Vector3 cameraVelocity = Vector3.zero;

    //* This aimOffset variable is used solely by the aiming system. By default, this value is always Vector3.zero.
    [NonSerialized] public Vector3 aimOffset = Vector3.zero;
    public Vector3 playerPositionOnScreen {get {
        return Camera.main.WorldToScreenPoint(playerTransform.position);
    }}
    public Vector3 screenCenter {get {
        return new Vector3 (
            Screen.width / 2,
            Screen.height / 2,
            0
        );
    }}


    /// <summary>
    /// * Sets up the references to the player and the camera follow object
    /// </summary>
    private void Start () {
        GameObject player = GameObject.Find("Player");
        playerTransform = player.transform;
        cameraFollow.position = playerTransform.position;
    }

    /// <summary>
    /// * Updates the the position and rotation of the camera <br/><br/>
    /// ? Note: playerOffset by default has a value of 0, and will only change while the gun is aiming.<br/>
    /// </summary>
    private void Update() {
        // Get target position for cameraFollow object
        Vector3 targetPosition = playerTransform.position + RotateToCameraForwardDirection(aimOffset + basePlayerOffset);

        // Smoothly move the cameraFollow object to the target position
        cameraFollow.position = Vector3.SmoothDamp(cameraFollow.position, targetPosition, ref playerFollowVelocity, smoothTime);

        // Smoothly move the camera to the cameraFollow object
        Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position,cameraFollow.position + offset, ref cameraVelocity, smoothTime);
        
        // Look at the cameraFollow object
        Camera.main.transform.LookAt(cameraFollow);
    }

    /// <summary>
    /// * Rotate the camera around player counter-clockwise
    /// </summary>
    /// <param name="degrees">Amount to rotate in degrees</param>
    public void RotateCounterclockwise(float degrees) {
        offset = Quaternion.Euler(0, -degrees, 0) * offset;
    }
    /// <summary>
    /// * Rotate the camera around player clockwise
    /// </summary>
    /// <param name="degrees">Amount to rotate in degrees</param>
    public void RotateClockwise(float degrees) {
        offset = Quaternion.Euler(0, degrees, 0) * offset;
    }

    /// <summary>
    /// * Rotates a vector to the camera's forward direction
    /// TODO: This will need a better name
    /// </summary>
    /// <param name="vector">The input vector</param>
    /// <returns>The rotated vector</returns>
    public Vector3 RotateToCameraForwardDirection(Vector3 vector) {
        Vector3 forwardDirection = Camera.main.transform.forward;
        forwardDirection.y = 0;
        return Quaternion.LookRotation(forwardDirection) * vector;
    }
}