using UnityEngine;
using UnityEngine.UI;

public class NodeConnection : MonoBehaviour
{
    private RectTransform lineRectTransform;
    private Image lineImage;
    private DADBase _originNode;
    private DADBase _endNode;

    public void Initialize(DADBase start, DADBase end)
    {
        print($"start: {start} end: {end}");
        _originNode = start;
        _endNode = end;
        this.transform.SetParent(start.transform);
        // Create a new GameObject with an Image component
        GameObject lineObject = new GameObject("Line");
        lineObject.transform.SetParent(transform);
        lineImage = lineObject.AddComponent<Image>();
        lineRectTransform = lineImage.rectTransform;

        // Set the line color and sprite
        lineImage.color = Color.black;
        lineImage.sprite = null; // You can assign a sprite if you want a textured line

        DrawLine(start.GetComponent<RectTransform>().position, end.GetComponent<RectTransform>().position);
    }

    private void DrawLine(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        // Set the position and size of the line
        lineRectTransform.sizeDelta = new Vector2(distance, 2f); // 2f is the line thickness
        lineRectTransform.pivot = new Vector2(0, 0.5f);
        lineRectTransform.position = start;
        lineRectTransform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
    }
}