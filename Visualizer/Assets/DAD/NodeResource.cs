using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YamlDotNet.RepresentationModel;
using System.IO;
using System;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using TMPro;
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
    [SerializeField] private TMP_Text _nameText;
    private string _yamlPath;
    private Transform _nodeZoneTransform;
    private Color _nodeColor;


    public void Initialize(string baseYamlPath, Transform nodeZoneTransform)
    {
        _nodeZoneTransform = nodeZoneTransform;
        _yamlPath = baseYamlPath;

        //get the name and color from the GUI section of the base graph
        YamlStream yamlStream = LoadYaml(_yamlPath);
        YamlNode configNode = FindYamlNodeByName(yamlStream, "config");

        //in the config section, search for the color and name. keep everything else as a node form
        //name, nickname, module are required 
        string hex = "#";
        string name = string.Empty;


        if (configNode is YamlMappingNode mappingNode)
        {
            foreach (var entry in mappingNode.Children)
            {
                if (entry.Key.ToString() == "color")
                {
                    hex += entry.Value.ToString();
                }
                else if (entry.Key.ToString() == "nickname")
                {
                    name = entry.Value.ToString();
                }
            }
        }


        if (!ColorUtility.TryParseHtmlString(hex, out _nodeColor))
        {
            _nodeColor = UnityEngine.Random.ColorHSV();
            print("could not parse color from graph. using random color.");
        }
        this.transform.GetComponent<Image>().color = _nodeColor;

        if (name != string.Empty)
        {
            _nameText.text = name;
        }
        else
        {
            //throw a fit because no name
        }


    }

    public static YamlStream LoadYaml(string path)
    {
        if (File.Exists(path))
        {
            string yamlContent = File.ReadAllText(path);
            var input = new StringReader(yamlContent);
            YamlStream yamlStream = new YamlStream();
            yamlStream.Load(input);
            return yamlStream;
        }
        return null;
    }

    public static YamlNode FindYamlNodeByName(YamlStream yamlStream, string name)
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
        node.GetComponent<NodeInstance>().Initialize(_yamlPath, _nodeColor, _nameText.text);
        GraphBuilder.AllNodes.Add(node.GetComponent<NodeInstance>());
    }

    public void OnDrag(PointerEventData eventData)
    {
    }
}

