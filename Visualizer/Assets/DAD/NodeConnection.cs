using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class NodeConnection : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text _connectionText;
    private RectTransform lineRectTransform;
    private Image lineImage;
    private NodeInstance _originNode;
    private NodeInstance _endNode;
    private bool _isSelected = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && _isSelected)
        {
            DestroyConnection();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            if (!RectTransformUtility.RectangleContainsScreenPoint(lineRectTransform, mousePosition, Camera.main))
            {
                Deselect();
            }
        }
    }

    private void Select()
    {
        _isSelected = true;
        _connectionText.color = Color.red;
    }

    private void Deselect()
    {
        _isSelected = false;
        _connectionText.color = Color.black;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isSelected)
        {
            Deselect();
        }
        else
        {
            Select();
        }
    }

    private void DestroyConnection()
    {
        GraphBuilder.AllConnections.Remove(this);
        _originNode.RemoveConnection(this);
        _endNode.RemoveConnection(this);
        Destroy(this.gameObject);
    }

    public void Initialize(NodeInstance start, NodeInstance end)
    {
        _originNode = start;
        _endNode = end;
        this.transform.SetParent(start.transform);
        // Create a new GameObject with an Image component
        lineImage = gameObject.GetComponent<Image>();
        lineRectTransform = lineImage.rectTransform;

        // Set the line color and sprite
        lineImage.color = Color.black;
        lineImage.sprite = null; // You can assign a sprite if you want a textured line

        UpdateLine();
    }

    public NodeInstance GetOrigin()
    {
        return _originNode;
    }

    public NodeInstance GetTerminal()
    {
        return _endNode;
    }

    public void UpdateLine()
    {
        Vector3 startPos = _originNode.CanvasPosition;
        Vector3 endPos = _endNode.CanvasPosition;

        Vector3 direction = endPos - startPos;
        float distance = direction.magnitude;

        // Set the position and size of the line
        lineRectTransform.sizeDelta = new Vector2(distance, 2f); // 2f is the line thickness
        lineRectTransform.pivot = new Vector2(0, 0.5f);
        lineRectTransform.position = startPos;
        lineRectTransform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
    }
}