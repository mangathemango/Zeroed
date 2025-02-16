using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
///* Manages the crosshair/cursor<br/><br/>
///
///? As per current version, the crosshair's mobility is limitted by 2 invisible diagonal lines, forming a V shape <br/>
///? from the player's position to both top corners of the screen. In this code, they're called "view diagonals".<br/>
///? When the crosshair moves to either side of the view diagonals, the camera will rotate 45 degress on that way<br/><br/>
///
/// </summary>
public class Crosshair : Singleton<Crosshair>
{   

    [Range(10f, 100f)]
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private Transform crosshair;
    private bool rotateCameraReady = true;
    private Transform playerTransform;

    void Start()
    {
        // Locks the cursor in the center of the screen
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Getting references
        crosshair.position = CameraManager.Instance.screenCenter;
        playerTransform = GameObject.Find("Player").transform;
    }
    
    void Update()
    {
        // Get mouse movement delta
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        crosshair.position += new Vector3(mouseX, mouseY, 0);
        ClampCrosshairOnViewDiagonals();
        
        if (crosshair.position.normalized == new Vector3(crosshair.position.x,minY,0).normalized && rotateCameraReady) {
            StartCoroutine(RotateCamera());
            rotateCameraReady = false;
        }

    }

    float minY;
    /// <summary>
    /// * Clamps the crosshair inside the view diagonals <br/><br/>
    /// 
    /// ? This is first done by calculating the minimum Y value the crosshair can have based on the X value. <br/>
    /// ? This is done in the getMinY function. <br/><br/>
    /// ? After that, the crosshair's position is clamped between the screen's edges and the minimum Y value. <br/>
    /// </summary>
    private void ClampCrosshairOnViewDiagonals() {
        minY = GetMinY(crosshair.position.x);
        crosshair.position = new Vector3(
            Mathf.Clamp(crosshair.position.x, 0  , Screen.width),
            Mathf.Clamp(crosshair.position.y,minY, Screen.height),
            0
        );
    }

    /// <summary>
    /// * Calculates the minimum Y value the crosshair can have based on the X value <br/><br/>
    /// 
    /// ! Trigger warning: Math. <br/><br/>
    ///
    /// ? First, let's consider a diagonal line from the bottom left corner of the screen to the top right corner of the screen. <br/>
    /// ? The function of this line is: f1(x) = y = x * Screen.height / Screen.width.  <br/>
    /// ? Similarly, let's consider the diagonal line from the top left corner to the bottom right corner <br/>
    /// ? The function of this line is: f2(x) = y = Screen.height - x * Screen.height / Screen.width.<br/>
    /// ? Therefore, to achieve our desired effect, we will use f1(x) if the crosshair is on the right half of the screen, <br/>
    /// ? and then use the f2(x) if the crosshair is on the left half of the screen. <br/><br/>
    /// 
    /// ? However, we want the V shape to be centered on the player's position on screen, but f1(x) and f2(x) intersects on the screen's center<br/>
    /// ? Therefore, we need to subtract minY by the distance between the player's position on screen and the screen center <br/><br/>
    /// 
    /// TODO: Right now, the view diagonals are dependent on the screen's resolution. Maybe change this to FOV next time?<br/>
    /// </summary>
    /// <param name="x">The x position of the crosshair</param>
    /// <returns>The lowest y position the cursor can go</returns>
    private float GetMinY(float x) {
        if (x > Screen.width / 2) {
            // Shot placement is on the right side of the screen
            minY = crosshair.position.x * Screen.height / Screen.width;
        } else {
            // Shot placement is on the left side of the screen
            minY = Screen.height - crosshair.position.x * Screen.height / Screen.width;
        }
        // Shift the center of the V shape to the player's position instead of the screen center
        minY = minY - (CameraManager.Instance.screenCenter.y - CameraManager.Instance.playerPositionOnScreen.y);

        // Limit the minimum Y value to the player's y position + 20 (crosshair going too close to the player bugs the game out)
        minY = Mathf.Max(minY, CameraManager.Instance.playerPositionOnScreen.y + 20);

        return minY;
    }
    private IEnumerator RotateCamera() {
        if (!rotateCameraReady) {
            yield break;
        }
        rotateCameraReady = false;
        Vector3 crosshairWorldpoint = CrosshairToRaycastHit().point;
        Vector3 lastCrosshairScreenPoint = crosshair.position;
        Vector3 lastPlayerPosition = playerTransform.position;
        if (crosshair.position.x > Screen.width / 2) {
            CameraManager.Instance.RotateClockwise(45);
        } else {
            CameraManager.Instance.RotateCounterclockwise(45);
        }
        float timeElapsed = 0;
        while (timeElapsed < 0.3f) {
            timeElapsed += Time.deltaTime;
            Vector3 currentPlayerPosition = playerTransform.position;
            crosshairWorldpoint += currentPlayerPosition - lastPlayerPosition;
            lastPlayerPosition = currentPlayerPosition;
            Vector3 currentCrosshairScreenPoint = Camera.main.WorldToScreenPoint(crosshairWorldpoint);
            crosshair.position += currentCrosshairScreenPoint - lastCrosshairScreenPoint;
            lastCrosshairScreenPoint = currentCrosshairScreenPoint;
            yield return null;
        }
        rotateCameraReady = true;
    }
    public Vector3 GetCrosshairDistanceFromCenter()
    {
        return new Vector3(
            (crosshair.position.x - Screen.width / 2) / Screen.width,
            (crosshair.position.y - Screen.height / 2) / Screen.height,
            0
        );
    }



    public Ray CrosshairToRay()
    {
        return Camera.main.ScreenPointToRay(crosshair.position);
    }


    public RaycastHit CrosshairToRaycastHit()
    {
        RaycastHit hit;
        Physics.Raycast(CrosshairToRay(), out hit);
        return hit;
    }

    public void Recoil(float recoilX, float recoilY)
    {
        crosshair.position = new Vector3(crosshair.position.x, crosshair.position.y + recoilY, 0);
    }
}
