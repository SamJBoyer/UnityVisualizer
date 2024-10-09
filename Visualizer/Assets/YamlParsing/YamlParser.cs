using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
using System.IO;
using System;
using YamlDotNet.Core.Events;
using System.Linq;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Core;

[ExecuteAlways]
public class YamlParser : MonoBehaviour
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