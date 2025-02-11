using System.Collections;
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
    private bool rotateCameraReady = true;
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

        shotPlacement.position += new Vector3(mouseX, mouseY, 0);
        shotOrigin.position = shotPlacement.position;

        float minY;
        if (shotPlacement.position.x > Screen.width / 2) {
            minY = shotOrigin.position.x * Screen.height / Screen.width;
        } else {
            minY = Screen.height - shotOrigin.position.x * Screen.height / Screen.width;
        }
        shotPlacement.position = new Vector3(
            Mathf.Clamp(shotPlacement.position.x, 0  , Screen.width),
            Mathf.Clamp(shotPlacement.position.y,Mathf.Clamp(minY, Screen.height / 2 + 20, Screen.height), Screen.height),
            0
        );
        
        shotOrigin.position = new Vector3(
            Mathf.Clamp(shotOrigin.position.x, 0  , Screen.width),
            Mathf.Clamp(shotOrigin.position.y,Mathf.Clamp(minY, Screen.height / 2 + 20, Screen.height), Screen.height),
            0
        );
        if (shotPlacement.position.normalized == new Vector3(shotPlacement.position.x,minY,0).normalized && rotateCameraReady) {
            StartCoroutine(RotateCamera());
        }

    }

    private IEnumerator RotateCamera() {
        if (!rotateCameraReady) {
            yield break;
        }
        rotateCameraReady = false;
        if (shotPlacement.position.x > Screen.width / 2) {
            CameraManager.Instance.RotateClockwise(45);
        } else {
            CameraManager.Instance.RotateCounterclockwise(45);
        }
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 targetDirection = Vector3.up;
        float targetMagnitude = (shotPlacement.position - screenCenter).magnitude;
        Vector3 targetPosition = screenCenter + targetDirection * targetMagnitude;
        shotPlacement.position = targetPosition;
        yield return new WaitForSeconds(0.1f);
        rotateCameraReady = true;
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
        shotPlacement.position = new Vector3(shotPlacement.position.x, shotPlacement.position.y + recoilY, 0);
    }
}