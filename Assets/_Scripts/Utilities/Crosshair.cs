using UnityEngine;

public class Crosshair : MonoBehaviour
{   
    void Start()
    {
        Cursor.visible = false;
    }
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0;
        transform.position = mousePos;
    }
}