using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float scrollSpeed = 20f;
    private Vector3 lastMousePosition;


    // Update is called once per frame
    void Update()
    {
        HandleMousePanning();
        HandleMouseScrolling();
    }

    void HandleMousePanning()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;
            Camera.main.transform.Translate(move, Space.World);
            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleMouseScrolling()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 move = scroll * scrollSpeed * Camera.main.transform.forward;
        Camera.main.transform.Translate(move, Space.World);
    }
}