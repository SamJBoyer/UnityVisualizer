using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using YamlDotNet.RepresentationModel;
using System.IO;
using System;
using YamlDotNet.Core.Events;
using System.Linq;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Core;
using UnityEngine.UI;

//instance of a node resource
public class DADBase : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerClickHandler
{
    public static DADBase DragOriginInstance;
    //flexible line used to draw between nodes
    [SerializeField] private GameObject linePrefab;
    //line that is completed once a connection between nodes is complete
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject _parameterObject;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isDrawingLine = false;
    private Vector3 startPosition;
    private LineConnection _lineDrawing;


    private static List<YamlRep> _yamlReps = new List<YamlRep>();
    //scalar nodes of the yaml
    private static List<YamlScalarNode> _scalarNodes = new List<YamlScalarNode>();
    [SerializeField] private Transform _contentTransform;
    [SerializeField] private GameObject _graphTextPrefab;
    [SerializeField] private GameObject _graphDropdownPrefab;

    private List<NodeConnection> _myConnections;


    //accept yaml stream
    public void Initialize(YamlNode yamlNode, Color color)
    {
        this.GetComponent<Image>().color = color;
        TraverseYaml(yamlNode, "");
    }

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        _parameterObject.SetActive(false);
        _lineDrawing = null;
        DragOriginInstance = null;
        _myConnections = new List<NodeConnection>();
    }

    private void TraverseYaml(YamlNode currentNode, string parentName)
    {
        switch (currentNode.NodeType)
        {
            case YamlNodeType.Scalar:
                // Set the anchor if it doesn't already have one
                if (currentNode.Anchor == null || currentNode.Anchor == "")
                {
                    currentNode.Anchor = Guid.NewGuid().ToString();
                }
                var scalar = (YamlScalarNode)currentNode;
                //Debug.Log($"scalar: {scalar.Value}");
                GameObject newUI = null;
                string value = scalar.Value.ToLower();
                string[] options = null;

                if (value.Contains("dropdown"))
                {
                    string arr = value.Split('-')[1];
                    options = arr.Trim('[', ']').Split(',').Select(option => option.Trim()).ToArray();
                    newUI = Instantiate(_graphDropdownPrefab, _contentTransform);
                }
                else if (value.Equals("text"))
                {
                    newUI = Instantiate(_graphTextPrefab, _contentTransform);
                }
                YamlRep yamlRep = newUI.GetComponent<YamlRep>();
                yamlRep.Initialize(parentName, currentNode.Anchor, options);
                _yamlReps.Add(yamlRep);
                _scalarNodes.Add(scalar);

                break;
            case YamlNodeType.Sequence:
                var sequence = (YamlSequenceNode)currentNode;
                foreach (var child in sequence.Children)
                {
                    TraverseYaml(child, "");
                }
                break;
            case YamlNodeType.Mapping:
                var mapping = (YamlMappingNode)currentNode;
                foreach (var entry in mapping.Children)
                {
                    var key = (YamlScalarNode)entry.Key;
                    //Debug.Log($"mapping:{key.Value}:");
                    TraverseYaml(entry.Value, key.Value);
                }
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            isDrawingLine = true;
            startPosition = ScreenToWorldPoint(rectTransform.position);
            GameObject newLine = Instantiate(linePrefab, startPosition, Quaternion.identity, transform);
            _lineDrawing = newLine.GetComponent<LineConnection>();
            _lineDrawing.StartLine(rectTransform);
            DragOriginInstance = this;
            print($"drag origin set {DragOriginInstance}");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        else if (eventData.button == PointerEventData.InputButton.Right && isDrawingLine)
        {
            Vector3 currentPosition = ScreenToWorldPoint(Input.mousePosition);
            _lineDrawing.DrawLine(currentPosition);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            _parameterObject.SetActive(!_parameterObject.activeSelf);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && isDrawingLine)
        {
            isDrawingLine = false;
            DragOriginInstance = null;
            Destroy(_lineDrawing.gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //if a line is being drawn to this node, make it an endpoint 
        if (DragOriginInstance != null && DragOriginInstance != this)
        {
            GameObject nodeConnection = Instantiate(nodePrefab);
            NodeConnection conn = nodeConnection.GetComponent<NodeConnection>();
            conn.Initialize(DragOriginInstance, this);
            _myConnections.Add(conn);
            DragOriginInstance = null;
        }
    }

    public Vector3 CanvasPosition()
    {
        return rectTransform.position;
    }


    private Vector3 ScreenToWorldPoint(Vector3 screenPosition)
    {
        Vector3 worldPosition;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), screenPosition, canvas.worldCamera, out worldPosition);
        return worldPosition;
    }
}