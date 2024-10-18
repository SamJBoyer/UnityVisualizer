using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using System.IO;

public class GraphBuilder : MonoBehaviour
{
    public static List<NodeConnection> AllConnections = new List<NodeConnection>();
    public static List<NodeInstance> AllNodes = new List<NodeInstance>();
    public string WritePath;

    //scroll content object where the nodes are loaded into the resource tray
    [SerializeField] private Transform _nodeResourceContent;
    //scroll content object where the nodes are organized and connections are drawn
    [SerializeField] private Transform _DADContent;
    [SerializeField] private GameObject _nodeResourcePrefab;

    private void Start()
    {
        //load yaml nodes into the resource tray
        string nodeDirectory = Path.Combine(Application.dataPath, "DAD", "Nodes");
        print(nodeDirectory);
        string[] nodeYamls = Directory.GetFiles(nodeDirectory);
        foreach (string file in nodeYamls)
        {
            if (Path.GetExtension(file) == ".yaml")
            {
                GameObject newNodeResource = Instantiate(_nodeResourcePrefab, _nodeResourceContent);
                newNodeResource.GetComponent<NodeResource>().Initialize(file, _DADContent);
            }
            else
            {
                Debug.LogWarning($"File {file} is not a YAML file");
            }
        }
    }

    public void BuildGraph()
    {
        print("b u i l d i n g");
        /*YamlSequenceNode baseSequence = new YamlSequenceNode();
        //collect all the yaml instances 
        foreach (var node in AllNodes)
        {
            //check if node is complete

            //insert input and outputs
            foreach (var connection in AllConnections)
            {
                if (connection.GetOrigin() == node)
                {
                    //add the connection to the node
                }
                else if (connection.GetTerminal() == node)
                {

                }
            }
            baseSequence.Add(node.UpdateAndGetNode());
        }
        YamlMappingNode nodeMap = new YamlMappingNode();
        nodeMap.Add("node", baseSequence);*/


        YamlStream yamlStream = new YamlStream(new YamlDocument(AllNodes[0].UpdateAndGetNode()));
        string path = Path.Combine(Application.dataPath, WritePath);
        using (var writer = new StreamWriter(path))
        {
            yamlStream.Save(writer);
        }
        Debug.Log($"YAML content written to {path}");
    }



}
