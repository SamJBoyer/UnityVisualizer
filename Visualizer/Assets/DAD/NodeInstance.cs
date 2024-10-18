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
using static NodeResource;
using TMPro;

//instance of a node resource
public class NodeInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerClickHandler
{
    public static NodeInstance DragOriginInstance;
    [SerializeField] private TMP_Text _nameText;
    //flexible line used to draw between nodes
    [SerializeField] private GameObject _linePrefab;
    //line that is completed once a connection between nodes is complete
    [SerializeField] private GameObject _redisConnectionPrefab;
    [SerializeField] private GameObject _parameterObject;


    private RectTransform rectTransform;
    private Canvas canvas;
    private bool _isDrawingLine = false;
    private Vector3 _startPosition;
    private LineConnection _lineDrawing;

    private List<YamlRep> _yamlReps = new List<YamlRep>();
    private List<YamlScalarNode> _scalarNodes = new List<YamlScalarNode>();

    [SerializeField] private Transform _contentTransform;
    [SerializeField] private GameObject _graphTextPrefab;
    [SerializeField] private GameObject _graphDropdownPrefab;

    private List<NodeConnection> _myConnections;
    private string _nickname;
    private YamlMappingNode _myNode;

    //accept yaml stream
    public void Initialize(string yamlPath, Color color, string name)
    {
        this.GetComponent<Image>().color = color;
        YamlStream yamlStream = LoadYaml(yamlPath);

        //find the config and rearrange 
        YamlNode configNode = FindYamlNodeByName(yamlStream, "config");

        //create a new node and reorg config data
        YamlMappingNode nameNode = null;
        YamlMappingNode moduleNode = null;

        if (configNode is YamlMappingNode mappingNode)
        {
            foreach (var entry in mappingNode.Children)
            {
                if (entry.Key.ToString() == "name")
                {
                    nameNode = new YamlMappingNode(entry.Key, entry.Value);
                }
                else if (entry.Key.ToString() == "nickname")
                {
                    _nickname = entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "module")
                {
                    moduleNode = new YamlMappingNode(entry.Key, entry.Value);
                }
            }
        }
        YamlNode parameterNode = FindYamlNodeByName(yamlStream, "parameters");
        TraverseYaml(parameterNode, "");
        _myNode = new YamlMappingNode(_nickname, nameNode, moduleNode, new YamlMappingNode("parameters", parameterNode));
        GraphBuilder.AllNodes.Add(this);
        _nickname = name;
        _nameText.text = _nickname;
    }

    public void RemoveConnection(NodeConnection connection)
    {
        _myConnections.Remove(connection);
    }

    public YamlNode UpdateAndGetNode()
    {
        Dictionary<AnchorName, string> repContent = _yamlReps.ToDictionary(x => x.GetAnchor(), x => x.GetContent());
        foreach (var node in _scalarNodes)
        {
            AnchorName anchor = node.Anchor;
            if (repContent.ContainsKey(anchor))
            {
                node.Value = repContent[anchor];
            }
            node.Anchor = AnchorName.Empty;
        }

        return _myNode;
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
                    currentNode.Anchor = new AnchorName(Guid.NewGuid().ToString());
                }
                var scalar = (YamlScalarNode)currentNode;
                Debug.Log($"scalar: {scalar.Value}");
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
            _isDrawingLine = true;
            _startPosition = ScreenToWorldPoint(rectTransform.position);
            GameObject newLine = Instantiate(_linePrefab, _startPosition, Quaternion.identity, transform);
            _lineDrawing = newLine.GetComponent<LineConnection>();
            _lineDrawing.StartLine(rectTransform);
            DragOriginInstance = this;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //drag in the env
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            foreach (var connection in _myConnections)
            {
                connection.UpdateLine();
            }
        }
        //draw connections
        else if (eventData.button == PointerEventData.InputButton.Right && _isDrawingLine)
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
        if (eventData.button == PointerEventData.InputButton.Right && _isDrawingLine)
        {
            _isDrawingLine = false;
            DragOriginInstance = null;
            Destroy(_lineDrawing.gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //if a line is being drawn to this node, make it an endpoint 
        if (DragOriginInstance != null && DragOriginInstance != this)
        {
            GameObject nodeConnection = Instantiate(_redisConnectionPrefab);
            NodeConnection conn = nodeConnection.GetComponent<NodeConnection>();
            conn.Initialize(DragOriginInstance, this);
            //add connection between both nodes
            _myConnections.Add(conn);
            DragOriginInstance.AddConnection(conn);
            DragOriginInstance = null;

        }
    }

    public void AddConnection(NodeConnection connection)
    {
        _myConnections.Add(connection);
    }

    public Vector3 CanvasPosition => rectTransform.position;

    private Vector3 ScreenToWorldPoint(Vector3 screenPosition)
    {
        Vector3 worldPosition;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), screenPosition, canvas.worldCamera, out worldPosition);
        return worldPosition;
    }
}