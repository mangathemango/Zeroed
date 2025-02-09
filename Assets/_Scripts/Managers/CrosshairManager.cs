using Unity.VisualScripting;
using UnityEngine;

public class Crosshair : Singleton<Crosshair>
{   
    [Range(0f, 0.5f)]
    [SerializeField] private float smoothTime = 0f;
    [Range(10f, 100f)]
    [SerializeField] private float sensitivity = 1f;
    private Transform shotPlacement;
    private Transform shotOrigin;

    public Transform placement {get {return shotPlacement;}}

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        shotPlacement = GameObject.Find("Shot Placement UI").transform;
        shotOrigin = GameObject.Find("Shot Origin UI").transform;
        shotPlacement.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        shotOrigin.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }

    void Update()
    {
        // Get mouse movement delta
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        
        // Calculate target position based on mouse movement
        shotOrigin.position = shotOrigin.position + new Vector3(mouseX, mouseY, 0);
        shotOrigin.position = new Vector3(
            Mathf.Clamp(shotOrigin.position.x, 0, Screen.width),
            Mathf.Clamp(shotOrigin.position.y, 0, Screen.height),
            0
        );

        shotPlacement.position = Vector3.SmoothDamp(shotPlacement.position, shotOrigin.position, ref velocity, smoothTime);

        // Clamp the crosshair position within the screen bounds
        shotPlacement.position = new Vector3(
            Mathf.Clamp(shotPlacement.position.x, 0, Screen.width),
            Mathf.Clamp(shotPlacement.position.y, 0, Screen.height),
            0
        );
    }

    public Vector3 GetDistanceFromCenter()
    {
        return new Vector3(
            (shotPlacement.position.x - Screen.width / 2) / Screen.width,
            (shotPlacement.position.y - Screen.height / 2) / Screen.height,
            0
        );
    }
    public Vector3 GetMaxDistanceFromCenter(Vector3 direction) {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 maxDistance = Vector3.zero;

        if (direction.x != 0)
        {
            float t = (direction.x > 0) ? (Screen.width - screenCenter.x) / direction.x : -screenCenter.x / direction.x;
            maxDistance = screenCenter + direction * t;
        }

        if (direction.y != 0)
        {
            float t = (direction.y > 0) ? (Screen.height - screenCenter.y) / direction.y : -screenCenter.y / direction.y;
            Vector3 candidate = screenCenter + direction * t;
            if (maxDistance == Vector3.zero || candidate.magnitude < maxDistance.magnitude)
            {
            maxDistance = candidate;
            }
        }
        maxDistance.x /= Screen.width;
        maxDistance.y /= Screen.height;
        return maxDistance;
    }
    public void Recoil(float recoilX, float recoilY)
    {
        
    }
}