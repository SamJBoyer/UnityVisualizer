using UnityEngine;
using UnityEngine.UI;

public class ViewController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 2.0f;

    private void Update()
    {
        // Zooming with mouse scroll wheel
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0)
        {
            Vector3 scale = content.localScale;
            scale += Vector3.one * scrollDelta * zoomSpeed;
            scale = new Vector3(
                Mathf.Clamp(scale.x, minZoom, maxZoom),
                Mathf.Clamp(scale.y, minZoom, maxZoom),
                1f
            );
            content.localScale = scale;
        }

        // Panning (dragging) the map
        if (Input.GetMouseButton(0)) // Left mouse button
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Start dragging
                Vector3 lastMousePosition = Input.mousePosition;
                content.anchoredPosition += (Vector2)(lastMousePosition - Input.mousePosition) * scrollRect.viewport.rect.width / 100;
            }
        }
    }
}
