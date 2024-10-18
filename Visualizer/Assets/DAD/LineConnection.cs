using UnityEngine;
using UnityEngine.UI;

public class LineConnection : MonoBehaviour
{
    private RectTransform _lineOrigin;
    private RectTransform lineRectTransform;
    private Image lineImage;

    public void StartLine(RectTransform originTransform)
    {
        _lineOrigin = originTransform;
        lineImage = this.gameObject.AddComponent<Image>();
        lineRectTransform = lineImage.rectTransform;
        // Set the line color and sprite
        lineImage.color = Color.black;
        lineImage.sprite = null; // You can assign a sprite if you want a textured line
    }

    public void DrawLine(Vector3 mousePosition)
    {
        Vector3 startPos = _lineOrigin.position;
        Vector3 direction = mousePosition - startPos;
        float distance = direction.magnitude;

        // Set the position and size of the line
        lineRectTransform.sizeDelta = new Vector2(distance, 2f); // 2f is the line thickness
        lineRectTransform.pivot = new Vector2(0, 0.5f);
        lineRectTransform.position = startPos;
        lineRectTransform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
    }
}