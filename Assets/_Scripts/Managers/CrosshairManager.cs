using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///* Manages the crosshair/cursor<br/><br/>
///
///! The crosshair currently has two components: the shot placement (square) and the shot origin (dot) <br/>
///! This is only implemented because of a previous recoil system. This will be merged into one later <br/><br/>
///
///? As per current version, the crosshair's mobility is limitted by 2 invisible diagonal lines,<br/>
///? from the player's position to both top corners to the screen. In this code, they're called "view diagonals"<br/>
///? When the crosshair moves to either side of the view diagonals, the camera will rotate 45 degress on that way<br/><br/>
///
/// TODO #1: Remove shot placement/ shot origin separation

/// </summary>
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
    private Transform player;
    [Range(1f, 10f)]
    [SerializeField] private float stablizeRate = 2f; //! This does nothing
    void Start()
    {
        // Locks the cursor in the center of the screen
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        shotPlacement = GameObject.Find("Shot Placement UI").transform;
        shotOrigin = GameObject.Find("Shot Origin UI").transform;
        shotPlacement.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        shotOrigin.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        smoothTime = minSmoothTime;
        player = GameObject.Find("Player").transform;
    }
    
    float minY;
    void Update()
    {
        // Get mouse movement delta
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        shotPlacement.position += new Vector3(mouseX, mouseY, 0);
        ClampCrosshairOnViewDiagonals();
        
        if (shotPlacement.position.normalized == new Vector3(shotPlacement.position.x,minY,0).normalized && rotateCameraReady) {
            StartCoroutine(RotateCamera());
        }

        shotOrigin.position = shotPlacement.position;

    }

    private void ClampCrosshairOnViewDiagonals() {
        if (shotPlacement.position.x > Screen.width / 2) {
            minY = shotOrigin.position.x * Screen.height / Screen.width;
        } else {
            minY = Screen.height - shotOrigin.position.x * Screen.height / Screen.width;
        }
        minY = minY - CameraManager.Instance.screenCenter.y + CameraManager.Instance.playerPositionOnScreen.y;
        shotPlacement.position = new Vector3(
            Mathf.Clamp(shotPlacement.position.x, 0  , Screen.width),
            Mathf.Clamp(shotPlacement.position.y,Mathf.Clamp(minY, CameraManager.Instance.playerPositionOnScreen.y + 20, Screen.height), Screen.height),
            0
        );
    }
    private IEnumerator RotateCamera() {
        if (!rotateCameraReady) {
            yield break;
        }
        rotateCameraReady = false;
        Vector3 crosshairWorldpoint = ShotPlacementToRaycastHit().point;
        Vector3 lastCrosshairScreenPoint = shotPlacement.position;
        Vector3 lastPlayerPosition = player.position;
        if (shotPlacement.position.x > Screen.width / 2) {
            CameraManager.Instance.RotateClockwise(45);
        } else {
            CameraManager.Instance.RotateCounterclockwise(45);
        }
        float timeElapsed = 0;
        while (timeElapsed < 0.3f) {
            timeElapsed += Time.deltaTime;
            Vector3 currentPlayerPosition = player.position;
            crosshairWorldpoint += currentPlayerPosition - lastPlayerPosition;
            lastPlayerPosition = currentPlayerPosition;
            Vector3 currentCrosshairScreenPoint = Camera.main.WorldToScreenPoint(crosshairWorldpoint);
            shotPlacement.position += currentCrosshairScreenPoint - lastCrosshairScreenPoint;
            lastCrosshairScreenPoint = currentCrosshairScreenPoint;
            yield return null;
        }
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

    public Ray ShotOriginToRay()
    {
        return Camera.main.ScreenPointToRay(shotOrigin.position);
    }

    public Ray ShotPlacementToRay()
    {
        return Camera.main.ScreenPointToRay(shotPlacement.position);
    }

    public RaycastHit ShotOriginToRaycastHit()
    {
        RaycastHit hit;
        Physics.Raycast(ShotOriginToRay(), out hit);
        return hit;
    }

    public RaycastHit ShotPlacementToRaycastHit()
    {
        RaycastHit hit;
        Physics.Raycast(ShotPlacementToRay(), out hit);
        return hit;
    }

    public void Recoil(float recoilX, float recoilY)
    {
        shotPlacement.position = new Vector3(shotPlacement.position.x, shotPlacement.position.y + recoilY, 0);
    }
}
