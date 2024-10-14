using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
using System.IO;
using System;
using YamlDotNet.Core.Events;
using System.Linq;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Core;

/// <summary>
/// author: Sam Boyer
/// gmail: sam.james.boyer@gmail.com
/// 
/// this script manages the node representation in the node tray
/// 
/// </summary>
public class NodeResource : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [SerializeField] private GameObject nodeInstancePrefab;
    private string _resourceDirectory;
    private YamlNode _parameterNode;
    private Transform _nodeZoneTransform;
    private Color _nodeColor;

    public void Initialize(string resourceDir, string baseYamlPath, Transform nodeZoneTransform)
    {
        _resourceDirectory = resourceDir;
        _nodeZoneTransform = nodeZoneTransform;

        if (File.Exists(baseYamlPath))
        {
            string yamlContent = File.ReadAllText(baseYamlPath);
            var input = new StringReader(yamlContent);
            YamlStream yamlStream = new YamlStream();
            yamlStream.Load(input);
            //get gui information from the base yaml
            YamlNode guiNode = FindYamlNodeByName(yamlStream, "GUI");
            string hex = "#";
            if (guiNode is YamlMappingNode mappingNode)
            {
                foreach (var entry in mappingNode.Children)
                {
                    if (entry.Key.ToString() == "color")
                    {
                        hex += entry.Value.ToString();
                    }
                }
            }
            if (!ColorUtility.TryParseHtmlString(hex, out _nodeColor))
            {
                _nodeColor = UnityEngine.Random.ColorHSV();
                print("could not parse color from graph. using random color.");
            }
            this.transform.GetComponent<Image>().color = _nodeColor;
            YamlNode parameterNode = FindYamlNodeByName(yamlStream, "parameters");
            _parameterNode = parameterNode;
        }
        else
        {
            Debug.LogError("YAML file not found.");
        }
    }



    private YamlNode FindYamlNodeByName(YamlStream yamlStream, string name)
    {
        foreach (YamlDocument document in yamlStream.Documents)
        {
            YamlMappingNode rootNode = (YamlMappingNode)document.RootNode;
            foreach (var entry in rootNode.Children)
            {
                if (entry.Key.ToString().ToLower() == name.ToLower())
                {
                    return entry.Value;
                }
            }
        }
        return null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //create a node instance 
        GameObject node = Instantiate(nodeInstancePrefab, this.transform.position, Quaternion.identity, _nodeZoneTransform);
        node.GetComponent<DADBase>().Initialize(_parameterNode, _nodeColor);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }
}


public class Balls : MonoBehaviour
{
    public string YAMLName;
    public string WritePath;
    private YamlStream _yamlStream;
    private static List<YamlRep> _yamlReps = new List<YamlRep>();
    private static List<YamlScalarNode> _scalarNodes = new List<YamlScalarNode>();


    [SerializeField] private Transform _scrollContentTransform;
    [SerializeField] private GameObject _graphTextPrefab;
    [SerializeField] private GameObject _graphDropdownPrefab;

    // Start is called before the first frame update
    public void LoadYaml()
    {
        string filePath = Path.Combine(Application.dataPath, YAMLName);
        if (File.Exists(filePath))
        {
            print("YAML file found.");
            string yamlContent = File.ReadAllText(filePath);
            var input = new StringReader(yamlContent);
            _yamlStream = new YamlStream();
            _yamlStream.Load(input);

            foreach (var document in _yamlStream.Documents)
            {
                TraverseYaml(document.RootNode, "");
            }
        }
        else
        {
            Debug.LogError("YAML file not found.");
        }
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
                    newUI = Instantiate(_graphDropdownPrefab, _scrollContentTransform);
                }
                else if (value.Equals("text"))
                {
                    newUI = Instantiate(_graphTextPrefab, _scrollContentTransform);
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
                    Debug.Log($"mapping:{key.Value}:");
                    TraverseYaml(entry.Value, key.Value);
                }
                break;
        }
    }

    public void WriteYamlToFile()
    {
        Dictionary<AnchorName, string> repContent = _yamlReps.ToDictionary(x => x.GetAnchor(), x => x.GetContent());
        foreach (var node in _scalarNodes)
        {
            AnchorName anchor = node.Anchor;
            if (repContent.ContainsKey(anchor))
            {
                node.Value = repContent[anchor];
                node.Anchor = null;
            }
        }

        // Write the updated YAML content to a file
        string path = Path.Combine(Application.dataPath, WritePath);
        using (var writer = new StreamWriter(path))
        {
            _yamlStream.Save(writer);
        }
        Debug.Log($"YAML content written to {path}");
    }
}