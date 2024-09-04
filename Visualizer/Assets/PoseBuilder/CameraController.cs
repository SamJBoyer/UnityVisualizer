using UnityEngine;
using static PoseBuilderUI;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 5.0f;
    public float panningSpeed = 0.5f;
    public float movementSpeed = 5.0f;
    public float zoomSpeed = 5.0f;

    private (Vector3, Quaternion)? savedPerspective;
    private (Vector3, Quaternion) defaultPerspective;
    private Vector3 lastMousePosition;

    void Start()
    {
        defaultPerspective = (new Vector3(0, 3.75f, -0.71f), Quaternion.Euler(12.14f, 0, 0));
    }

    void Update()
    {
        if (DOFChoosingMode || FocusedOnUI || InGameView) return;

        // Camera rotation
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float angleY = delta.x * rotationSpeed * Time.deltaTime;
            float angleX = -delta.y * rotationSpeed * Time.deltaTime;

            transform.eulerAngles += new Vector3(angleX, angleY, 0);
        }

        // Camera panning
        if (Input.GetMouseButton(0)) // Left mouse button
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x * panningSpeed * Time.deltaTime, -delta.y * panningSpeed * Time.deltaTime, 0);
            transform.Translate(move, Space.Self);
        }

        // Camera movement with arrow keys only
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;

        h *= movementSpeed * Time.deltaTime;
        v *= movementSpeed * Time.deltaTime;

        transform.Translate(new Vector3(h, 0, v), Space.Self);

        // Camera zooming with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(new Vector3(0, 0, scroll * zoomSpeed * Time.deltaTime), Space.Self);

        lastMousePosition = Input.mousePosition;
    }

    public void SetCameraToDOF()
    {
        transform.position = new Vector3(1, 3.88f, 0.53f);
        transform.eulerAngles = new Vector3(38.09f, 180, 0);
    }

    public void SetCameraToGameView()
    {
        savedPerspective = (transform.position, transform.rotation);
        (Vector3 position, Quaternion rotation) = defaultPerspective;
        transform.position = position;
        transform.rotation = rotation;
    }

    public void ReturnCameraToPrevious()
    {
        if (savedPerspective != null)
        {
            (Vector3 position, Quaternion rotation) = savedPerspective ?? defaultPerspective;
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}