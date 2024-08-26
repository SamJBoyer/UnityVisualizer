using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOFManager : MonoBehaviour
{
    public static HashSet<ReactiveTarget> AllTargets = new HashSet<ReactiveTarget>();
    public static HashSet<ReactiveTarget> SelectedTargets = new HashSet<ReactiveTarget>();

    private Vector2 startScreenPosition;
    private Vector2 currentScreenPosition;
    private bool isDrawingBox = false;
    private bool isSelecting = false;
    private Vector2 mousePosition1;


    void Update()
    {
        if (!PoseBuilderUI.DOFChoosingMode) return;

        // Check if the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the main camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("DOF"))
            {
                hit.collider.GetComponent<ReactiveTarget>().ToggleSelection();
            }
            isSelecting = true;
            mousePosition1 = Input.mousePosition;
            startScreenPosition = Input.mousePosition; // Capture start position
            isDrawingBox = true;
        }

        // Update current mouse position for drawing the box
        if (isSelecting)
        {
            currentScreenPosition = Input.mousePosition;
        }

        // End selection
        if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            isSelecting = false;
            SelectObjects();
            isDrawingBox = false; // Stop drawing the box
        }
    }

    void SelectObjects()
    {
        Vector3 mousePosition2 = Input.mousePosition;

        // Convert screen positions to world space
        Vector3 p1 = Camera.main.ScreenToViewportPoint(mousePosition1);
        Vector3 p2 = Camera.main.ScreenToViewportPoint(mousePosition2);

        // Calculate corners of the selection box in viewport space
        Vector3 topLeft = Vector3.Min(p1, p2);
        Vector3 bottomRight = Vector3.Max(p1, p2);

        // Find objects within the selection box
        foreach (ReactiveTarget target in FindObjectsOfType<ReactiveTarget>())
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(target.transform.position);
            if (viewportPos.x >= topLeft.x && viewportPos.x <= bottomRight.x && viewportPos.y >= topLeft.y && viewportPos.y <= bottomRight.y)
            {
                target.Select();
            }
        }
    }

    //draw the selection box on the screen
    void OnGUI()
    {
        if (isDrawingBox)
        {
            // Convert positions to GUI space
            var rect = Utils.GetScreenRect(startScreenPosition, currentScreenPosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f)); // Draw semi-transparent box
            Utils.DrawScreenRectBorder(rect, 2, Color.blue); // Draw border for the box
        }
    }

}

// Utility class for drawing rectangles and borders
public static class Utils
{
    public static Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector2.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector2.Max(screenPosition1, screenPosition2);
        // Create rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }



    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Draw top
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Draw left
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Draw right
        Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Draw bottom
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }
}