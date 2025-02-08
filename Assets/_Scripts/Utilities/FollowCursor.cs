using UnityEngine;

class FollowCursor:MonoBehaviour {
    Vector3 offset = new Vector3(-8,8,-8);
    void Update () {
        Vector3 targetPosition = new Vector3();
        Ray ray = Camera.main.ScreenPointToRay(Crosshair.Instance.transform.position);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit)) {
            targetPosition = raycastHit.point;
        }
        transform.position = targetPosition;
    }
}