using Unity.VisualScripting;
using UnityEngine;

public class Crosshair : Singleton<Crosshair>
{   
    [Range(0f, 0.5f)]
    [SerializeField] private float minSmoothTime = 0f;
    [Range(10f, 100f)]
    [SerializeField] private float sensitivity = 1f;
    private Transform shotPlacement;
    private Transform shotOrigin;

    public Transform placement {get {return shotPlacement;}}
    public bool shooting = false;
    private Vector3 velocity = Vector3.zero;
    private float smoothTime;
    [Range(1f, 10f)]
    [SerializeField] private float stablizeRate = 2f;
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        shotPlacement = GameObject.Find("Shot Placement UI").transform;
        shotOrigin = GameObject.Find("Shot Origin UI").transform;
        shotPlacement.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        shotOrigin.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        smoothTime = minSmoothTime;
    }

    void Update()
    {
        // Get mouse movement delta
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        if (!Input.GetMouseButton(0)) {
            shotOrigin.position = shotOrigin.position + new Vector3(mouseX, mouseY, 0);
            shotPlacement.position = Vector3.SmoothDamp(shotPlacement.position, shotOrigin.position, ref velocity, smoothTime);
            smoothTime = minSmoothTime;
        } else {
            shotPlacement.position = shotPlacement.position + new Vector3(mouseX, mouseY, 0);
            if (smoothTime > minSmoothTime)
            {
                smoothTime -= stablizeRate * Time.deltaTime;
            } else {
                smoothTime = minSmoothTime;
            }
            shotPlacement.position = Vector3.SmoothDamp(shotPlacement.position, shotOrigin.position, ref velocity, smoothTime);
        }



        shotOrigin.position = new Vector3(
            Mathf.Clamp(shotOrigin.position.x, 0, Screen.width),
            Mathf.Clamp(shotOrigin.position.y, 0, Screen.height),
            0
        );

        shotPlacement.position = new Vector3(
            Mathf.Clamp(shotPlacement.position.x, 0, Screen.width),
            Mathf.Clamp(shotPlacement.position.y, 0, Screen.height),
            0
        );
        
    }

    public Vector3 GetPlacementDistanceFromCenter()
    {
        return new Vector3(
            (shotPlacement.position.x - Screen.width / 2) / Screen.width,
            (shotPlacement.position.y - Screen.height / 2) / Screen.height,
            0
        );
    }
    public Vector3 GetOriginDistanceFromCenter()
    {
        return new Vector3(
            (shotOrigin.position.x - Screen.width / 2) / Screen.width,
            (shotOrigin.position.y - Screen.height / 2) / Screen.height,
            0
        );
    }
    public void Recoil(float recoilX, float recoilY)
    {
        Vector3 previousPosition = shotPlacement.position;
        shotPlacement.position -= GetPlacementDistanceFromCenter().normalized * recoilY;
        shotPlacement.RotateAround(shotPlacement.position, Vector3.forward, recoilX);
        smoothTime += (shotPlacement.position - previousPosition).magnitude / 100;
    }
}