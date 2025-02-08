using UnityEngine;

public class Crosshair : Singleton<Crosshair>
{   
    [Range(0f, 0.5f)]
    [SerializeField] private float smoothTime = 0.1f;
    [Range(10f, 100f)]
    [SerializeField] private float sensitivity = 1f;
    public Vector3 recoil;
    private Vector3 velocity = Vector3.zero;
    public Vector3 targetPosition = Vector3.zero;
    private Camera mainCamera;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        targetPosition = transform.position;
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Get mouse movement delta
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Calculate target position based on mouse movement
        targetPosition = targetPosition + new Vector3(mouseX, mouseY, 0) + recoil;
        targetPosition.x = Mathf.Clamp(targetPosition.x, 0, Screen.width);
        targetPosition.y = Mathf.Clamp(targetPosition.y, 0, Screen.height);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // Clamp the crosshair position within the screen bounds
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, 0, Screen.width),
            Mathf.Clamp(transform.position.y, 0, Screen.height),
            0
        );
        recoil = Vector3.zero;
    }
}